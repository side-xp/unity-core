using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Utility class for tracking changes on scripts and implemented types.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Project)]
    public class TypesMigration : ScriptableObject, IEditorConfig
    {

        #region Subclasses

        /// <summary>
        /// Stores the current and previous <see cref="Type.AssemblyQualifiedName"/>s of a type.
        /// </summary>
        [System.Serializable]
        private class TypeMigrationHistory
        {

            /// <summary>
            /// The GUID of the script asset that declares this type.
            /// </summary>
            [SerializeField]
            private string _scriptGUID = null;

            /// <summary>
            /// The list of the previous <see cref="Type.AssemblyQualifiedName"/>s of this type in the history of the project.
            /// </summary>
            [SerializeField]
            private string[] _previousAssemblyQualifiedNames = null;

            /// <summary>
            /// The current <see cref="Type.AssemblyQualifiedName"/> of this type.
            /// </summary>
            [SerializeField]
            private string _currentAssemblyQualifiedName = null;

            /// <summary>
            /// The reference of this named type.
            /// </summary>
            private Type _type = null;

            /// <inheritdoc cref="TypeMigrationHistory"/>
            private TypeMigrationHistory() { }

            /// <inheritdoc cref="TypeMigrationHistory"/>
            /// <param name="type">The type of which to track the changes.</param>
            /// <param name="scriptGUID"><inheritdoc cref="_scriptGUID" path="/summary"/></param>
            public TypeMigrationHistory(Type type, string scriptGUID)
            {
                _type = type;
                _scriptGUID = scriptGUID;
                _previousAssemblyQualifiedNames = new string[0];
                _currentAssemblyQualifiedName = type.AssemblyQualifiedName;
            }

            /// <inheritdoc cref="_scriptGUID"/>
            public string ScriptGUID => _scriptGUID;

            /// <inheritdoc cref="_previousAssemblyQualifiedNames"/>
            public string[] PreviousAssemblyQualifiedNames => _previousAssemblyQualifiedNames;

            /// <inheritdoc cref="_currentAssemblyQualifiedName"/>
            public string CurrentAssemblyQualifiedName => _currentAssemblyQualifiedName;

            /// <inheritdoc cref="_type"/>
            public Type Type
            {
                get
                {
                    if (_type == null)
                        _type = Type.GetType(_currentAssemblyQualifiedName);
                    return _type;
                }
            }

            /// <summary>
            /// Updates the data of this type.
            /// </summary>
            /// <returns>Returns true if the type is still valid, or false if the type is not valid or its containing script has been
            /// doesn't exist.</returns>
            public bool Update()
            {
                string path = AssetDatabase.GUIDToAssetPath(_scriptGUID);
                // Cancel if the script is not valid
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                // Cancel if the type declared in the script is not valid
                if (!ScriptUtility.GetScriptType(path, out Type declaredType))
                    return false;

                // Stop if the current data is still up to date
                if (_currentAssemblyQualifiedName == declaredType.AssemblyQualifiedName)
                    return true;

                // Register the current name as previous name if applicable
                if (!string.IsNullOrWhiteSpace(_currentAssemblyQualifiedName))
                {
                    using (var previousNames = new ListPoolScope<string>())
                    {
                        previousNames.AddRange(_previousAssemblyQualifiedNames);
                        previousNames.Add(_currentAssemblyQualifiedName);
                        _previousAssemblyQualifiedNames = previousNames.ToArray();
                    }
                }

                _currentAssemblyQualifiedName = declaredType.AssemblyQualifiedName;
                _type = declaredType;
                return true;
            }

        }

        #endregion


        #region Fields

        /// <summary>
        /// Flag enabled after tracked types have been reloaded.
        /// </summary>
        private static bool s_didReload = false;

        /// <summary>
        /// The list of all the tracked types and their data.
        /// </summary>
        [SerializeField]
        private List<TypeMigrationHistory> _trackedTypes = new List<TypeMigrationHistory>();

        #endregion


        #region Public API

        /// <summary>
        /// Gets the loaded config or load them from disk if not already.
        /// </summary>
        public static TypesMigration Instance => EditorConfigUtility.Get<TypesMigration>();

        /// <inheritdoc cref="Instance"/>
        public static TypesMigration I => Instance;

        /// <summary>
        /// Reloads the tracked types data to sync potential name changes.
        /// </summary>
        public static void Reload()
        {
            // Cancel if the tracked types have already been reloaded.
            if (s_didReload)
                return;

            // For each tracked type data
            foreach (TypeMigrationHistory data in I._trackedTypes)
                data.Update();
        }

        /// <summary>
        /// Gets the <see cref="Type"/> or a tracked script based on a given <see cref="Type.AssemblyQualifiedName"/>. If that name doesn't
        /// exist anymore (meaning the original type has been renamed or removed), this utility will try to get the new <see cref="Type"/>
        /// based on its previous names.
        /// </summary>
        /// <param name="typeName">The <see cref="Type.AssemblyQualifiedName"/> of a <see cref="Type"/> to resolve. If this given type name
        /// is not valid anymore (meaning the type has been renamed or removed), this utility will try to retrieve the related
        /// <see cref="Type"/> based on its previous names.</param>
        /// <returns>Returns the resolved <see cref="Type"/>.</returns>
        public static Type Resolve(string typeName)
        {
            Reload();

            // Cancel if the given type name is not valid
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            // Try to get type from the last registered name
            foreach (TypeMigrationHistory data in I._trackedTypes)
            {
                if (data.CurrentAssemblyQualifiedName == typeName)
                    return data.Type;
            }

            // Try to get type from its previous names
            foreach (TypeMigrationHistory data in I._trackedTypes)
            {
                foreach (string previousName in data.PreviousAssemblyQualifiedNames)
                {
                    if (previousName == typeName)
                        return data.Type;
                }
            }

            Type type = Type.GetType(typeName);
            // If the given type name is valid but the type is not tracked yet
            if (type != null && ScriptUtility.GetScriptPath(type, out string scriptPath))
            {
                // Try to get the GUID of the script that declares the given type
                string guid = AssetDatabase.AssetPathToGUID(scriptPath);
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    // Add the new tracked type data to the list and save it
                    TypeMigrationHistory data = new TypeMigrationHistory(type, guid);
                    I._trackedTypes.Add(data);
                    EditorConfigUtility.Save(I);
                    return type;
                }
            }

            return null;
        }

        /// <param name="type">Outputs the resolved <see cref="Type"/>.</param>
        /// <inheritdoc cref="Resolve(string)"/>
        public static bool Resolve(string typeName, out Type type)
        {
            type = Resolve(typeName);
            return type != null;
        }

        /// <summary>
        /// Makes sure that the changes of a given <see cref="Type"/> are tracked, and returns its <see cref="Type.AssemblyQualifiedName"/>.
        /// </summary>
        /// <param name="type">The type of which to track the changes.</param>
        /// <returns>Returns the <see cref="Type.AssemblyQualifiedName"/> of the given <see cref="Type"/>.</returns>
        public static string Resolve(Type type)
        {
            Resolve(type.AssemblyQualifiedName);
            return type.AssemblyQualifiedName;
        }

        /// <inheritdoc cref="IEditorConfig.PostLoad"/>
        public void PostLoad() { }

        #endregion

    }

}