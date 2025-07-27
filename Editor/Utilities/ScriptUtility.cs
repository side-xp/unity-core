using System;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;

using SideXP.Core.Reflection;
using System.Collections.Generic;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Miscellaneous functions for working with scripts in the project.
    /// </summary>
    public static class ScriptUtility
    {

        /// <summary>
        /// The extension of Assembly Definition assets (with "." prefix).
        /// </summary>
        public const string AssemblyDefinitionExtension = ".asmdef";

        /// <summary>
        /// The extension of C# script assets (with "." prefix).
        /// </summary>
        public const string CSExtension = ".cs";

        #region Parsing

        /// <summary>
        /// The pattern to isolate the root namespace declared in an Assembly Definition file.
        /// </summary>
        private static readonly Regex AsmdefRootNamespacePattern = new Regex(@"""rootNamespace"":\s*""(?<namespace>[A-Za-z0-9_.]+)""");

        /// <summary>
        /// Pattern used to extract the first namespace declared in a script.
        /// </summary>
        public static readonly Regex NamespaceDeclarationPattern = new Regex(@"namespace (?<namespace>[A-Za-z0-9_.]+)");

        /// <summary>
        /// Pattern used to extract the first class declared in a script, and its base type if applicable.
        /// </summary>
        public static readonly Regex ClassDeclarationPattern = new Regex(@"(?:public|internal) +(?:(?:static|abstract|sealed) +)?(?:class|struct|interface|enum) +(?<class>[A-Za-z0-9_]+)(?>\s+:\s+(?<base>[A-Za-z0-9_]+))?");

        /// <summary>
        /// Gets the namespace that should be used for a file at a given path.
        /// </summary>
        /// <param name="path">The path to the file of which to get the namespace to use. This path must be relative to the project's
        /// directory.</param>
        /// <returns>Returns the namespace defined in the first assembly definition file found in the hierarchy, or the root namespace
        /// defined in the editor settings.</returns>
        public static string GetNamespaceFromPath(string path)
        {
            // Cancel if the given path is not relative to this project
            if (!PathUtility.IsProjectPath(path))
                return null;

            path = PathUtility.ToRelativePath(path, true);
            // Gets the directory path
            if (!string.IsNullOrWhiteSpace(Path.GetExtension(path)))
                path = Path.GetDirectoryName(path);

            // While the directory path is valid
            while (!string.IsNullOrWhiteSpace(path))
            {
                // Check files in the current directory
                string absPath = PathUtility.ToAbsolutePath(path);
                foreach (string filepath in Directory.GetFiles(absPath))
                {
                    // Skip if the current file is not an Assembly Definition asset
                    if (Path.GetExtension(filepath) != AssemblyDefinitionExtension)
                        continue;

                    // Try to get the declared root namespace from the Assembly Definition
                    string content = File.ReadAllText(filepath);
                    Match rootNamespaceMatch = AsmdefRootNamespacePattern.Match(content);
                    return rootNamespaceMatch.Success ? rootNamespaceMatch.Groups["namespace"].Value : null;
                }

                // Move path to parent directory
                path = Path.GetDirectoryName(path);
            }

            return EditorSettings.projectGenerationRootNamespace;
        }

        /// <param name="asset">The from which to extract the declared type.</param>
        /// <inheritdoc cref="TryGetDeclaredType(string, out Type)"/>
        public static bool TryGetDeclaredType(TextAsset asset, out Type type)
        {
            if (asset is TextAsset textAsset)
                return TryGetDeclaredType(textAsset.text, out type);

            type = null;
            return false;
        }

        /// <summary>
        /// Tries to extract the main type declared in a script at the given path.
        /// </summary>
        /// <param name="path">The path to the script asset.</param>
        /// <param name="type">Outputs the extracted type.</param>
        /// <returns>Returns true if a type has been extracted successfully.</returns>
        public static bool TryGetDeclaredTypeFromScriptAt(string path, out Type type)
        {
            // Cancel if the file at the given path doesn't exist
            if (!File.Exists(path.ToAbsolutePath()))
            {
                type = null;
                return false;
            }

            // Cancel if the file is not a C# script
            if (Path.GetExtension(path) != CSExtension)
            {
                type = null;
                return false;
            }

            string content = File.ReadAllText(path.ToAbsolutePath());
            return TryGetDeclaredType(content, out type);
        }

        /// <summary>
        /// Tries to extract the main type declared in a given script content.
        /// </summary>
        /// <param name="content">The content of the script from which to extract the main type.</param>
        /// <param name="type">Outputs the extracted type.</param>
        /// <returns>Returns true if a type has been extracted successfully.</returns>
        public static bool TryGetDeclaredType(string content, out Type type)
        {
            // Cancel if the content of the script is not valid
            if (string.IsNullOrEmpty(content))
            {
                type = null;
                return false;
            }

            Match match = ClassDeclarationPattern.Match(content);

            // Skip if no declared class or struct can be found in the script
            if (!match.Success)
            {
                type = null;
                return false;
            }

            Match namespaceMatch = NamespaceDeclarationPattern.Match(content);
            string namespaceValue = namespaceMatch.Success ? namespaceMatch.Groups["namespace"].Value : null;

            if (!ReflectionUtility.FindType(match.Groups["class"].Value, namespaceValue, out type))
                type = null;

            return type != null;
        }

        #endregion


        #region Scripts Info

        /// <summary>
        /// Represents a reference to a script in the project.
        /// </summary>
        public class ScriptRef
        {

            /// <summary>
            /// The path to the script asset, relative to project's directory.
            /// </summary>
            private string _scriptPath = null;

            /// <summary>
            /// The content of the script as plain text.
            /// </summary>
            /// <remarks>Since we must read the script to actually extract its <see cref="Type"/>, we can store its content in the process,
            /// so we can use it in assets processor or any tool related to scripts.</remarks>
            private string _scriptContent = null;

            /// <summary>
            /// The type extracted from this script.
            /// </summary>
            private Type _type = null;

            /// <inheritdoc cref="ScriptRef"/>
            /// <param name="scriptPath"><inheritdoc cref="_scriptPath" path="/summary"/></param>
            public ScriptRef(string scriptPath)
            {
                _scriptPath = scriptPath.ToRelativePath();
            }

            /// <summary>
            /// Loads the <see cref="MonoScript"/> that declares this script.
            /// </summary>
            public MonoScript ScriptAsset => AssetDatabase.LoadAssetAtPath<MonoScript>(_scriptPath);

            /// <inheritdoc cref="_scriptPath"/>
            public string ScriptPath => _scriptPath;

            /// <inheritdoc cref="_scriptContent"/>
            public string ScriptContent
            {
                get
                {
                    if (_scriptContent == null)
                    {
                        string path = Path.GetFullPath(_scriptPath.ToAbsolutePath());
                        try
                        {
                            // Cancel if the file doesn't exist
                            if (!File.Exists(path))
                                return null;

                            _scriptContent = File.ReadAllText(_scriptPath);
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.LogException(e);
                            return null;
                        }
                    }
                    return _scriptContent;
                }
            }

            /// <summary>
            /// Gets the declared type in a script file.
            /// </summary>
            /// <remarks>
            /// Unity API has a <see cref="MonoScript.GetClass"/> function, but it doesn't work with structs and interfaces. This function uses
            /// both regular expressions and reflection, and so is able to get the declared type whatever its nature.<br/>
            /// This also applies to <see cref="UnityEngine.TextAsset.text"/>, which logs an error that can't be handled as an exception if the
            /// asset is about to be deleted. We prefer using a custom implementation to read the script, making this function actually usable
            /// in an OnWillDelete() message.
            /// </remarks>
            /// <returns>Returns the extracted type.</returns>
            public Type GetScriptType()
            {
                // Cancel if the content of the script has already been read (meaning the type has already been extracted or failed)
                if (!string.IsNullOrEmpty(_scriptContent))
                    return _type;

                // Check for cache data
                if (ScriptCache.Get(_scriptPath, out _type, out _))
                    return _type;

                // Cancel if the path doesn't lead to a *.cs script
                if (Path.GetExtension(_scriptPath) != CSExtension)
                    return _type;

                Match match = ClassDeclarationPattern.Match(ScriptContent);

                // Skip if no declared class or struct can be found in the script
                if (!match.Success)
                    return _type;

                Match namespaceMatch = NamespaceDeclarationPattern.Match(ScriptContent);
                string namespaceValue = namespaceMatch.Success ? namespaceMatch.Groups["namespace"].Value : null;

                if (!ReflectionUtility.FindType(match.Groups["class"].Value, namespaceValue, out _type))
                    _type = null;

                // Write cache data for future operations
                if (_type != null)
                    ScriptCache.Set(_scriptPath, _type);

                return _type;
            }

            /// <returns>Returns true if this script's type has been extracted successfully.</returns>
            /// <inheritdoc cref="GetScriptType()"/>
            public bool GetScriptType(out Type type)
            {
                type = GetScriptType();
                return type != null;
            }

        }

        /// <summary>
        /// Informations about scripts in the project.
        /// </summary>
        private static List<ScriptRef> s_scriptRefs = null;

        /// <summary>
        /// Gets a <see cref="ScriptRef"/> instance that represents the script at a given path.
        /// </summary>
        /// <param name="scriptPath">The path to the script asset.</param>
        /// <returns>Returns the found <see cref="ScriptRef"/>.</returns>
        public static ScriptRef GetScriptRef(string scriptPath)
        {
            scriptPath = scriptPath.ToRelativePath();
            return GetScriptRefsList().Find(i => i.ScriptPath == scriptPath);
        }

        /// <param name="scriptRef">Outputs the found or created <see cref="ScriptRef"/>.</param>
        /// <returns>Returns true if a <see cref="ScriptRef"/> has been found or created for the script at the given path.</returns>
        /// <inheritdoc cref="GetScriptRef(string)"/>
        public static bool GetScriptRef(string scriptPath, out ScriptRef scriptRef)
        {
            scriptRef = GetScriptRef(scriptPath);
            return scriptRef != null;
        }

        /// <summary>
        /// Gets (or creates) a <see cref="ScriptRef"/> instance that represents a given script asset.
        /// </summary>
        /// <param name="scriptAsset">The asset that declares the script.</param>
        /// <inheritdoc cref="GetScriptRef(string)"/>
        public static ScriptRef GetScriptRef(MonoScript scriptAsset)
        {
            return GetScriptRef(AssetDatabase.GetAssetPath(scriptAsset));
        }

        /// <inheritdoc cref="GetScriptRef(MonoScript)"/>
        /// <inheritdoc cref="GetScriptRef(string, out ScriptRef)"/>
        public static bool GetScriptRef(MonoScript scriptAsset, out ScriptRef scriptRef)
        {
            scriptRef = GetScriptRef(scriptAsset);
            return scriptRef != null;
        }

        /// <summary>
        /// Gets (or creates) a <see cref="ScriptRef"/> instance that represents the script that declares a given type.
        /// </summary>
        /// <param name="type">The type from which to get the script ref.</param>
        /// <inheritdoc cref="GetScriptRef(string)"/>
        public static ScriptRef GetScriptRef(Type type)
        {
            return GetScriptRefsList().Find(i => i.GetScriptType(out Type tmpType) && tmpType == type);
        }

        /// <inheritdoc cref="GetScriptRef(string, out ScriptRef)"/>
        /// <inheritdoc cref="GetScriptRef(Type)"/>
        public static bool GetScriptRef(Type type, out ScriptRef scriptRef)
        {
            scriptRef = GetScriptRef(type);
            return scriptRef != null;
        }

        /// <inheritdoc cref="ScriptRef.GetScriptType()"/>
        /// <inheritdoc cref="GetScriptRef(string)"/>
        public static Type GetScriptType(string scriptPath)
        {
            return GetScriptRef(scriptPath, out ScriptRef scriptRef) ? scriptRef.GetScriptType() : null;
        }

        /// <inheritdoc cref="ScriptRef.GetScriptType(out Type)"/>
        /// <inheritdoc cref="GetScriptRef(string)"/>
        public static bool GetScriptType(string scriptPath, out Type type)
        {
            type = null;
            return GetScriptRef(scriptPath, out ScriptRef scriptRef) && scriptRef.GetScriptType(out type);
        }

        /// <inheritdoc cref="ScriptRef.GetScriptType()"/>
        /// <inheritdoc cref="GetScriptRef(MonoScript)"/>
        public static Type GetScriptType(MonoScript scriptAsset)
        {
            return GetScriptRef(scriptAsset, out ScriptRef scriptRef) ? scriptRef.GetScriptType() : null;
        }

        /// <inheritdoc cref="ScriptRef.GetScriptType(out Type)"/>
        /// <inheritdoc cref="GetScriptRef(MonoScript)"/>
        public static bool GetScriptType(MonoScript scriptAsset, out Type type)
        {
            type = null;
            return GetScriptRef(scriptAsset, out ScriptRef scriptRef) && scriptRef.GetScriptType(out type);
        }

        /// <summary>
        /// Gets the path to the script that declares a given <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type of the script to get.</param>
        /// <returns>Returns the found script's path that declares the given <see cref="Type"/>.</returns>
        public static string GetScriptPath(Type type)
        {
            return GetScriptRef(type, out ScriptRef scriptRef) ? scriptRef.ScriptPath : null;
        }

        /// <param name="scriptPath">Outputs the found script's path that declares the given <see cref="Type"/>.</param>
        /// <inheritdoc cref="GetScriptRef(Type, out ScriptRef)"/>
        /// <inheritdoc cref="GetScriptPath(Type)"/>
        public static bool GetScriptPath(Type type, out string scriptPath)
        {
            scriptPath = GetScriptRef(type, out ScriptRef scriptRef) ? scriptRef.ScriptPath : null;
            return !string.IsNullOrEmpty(scriptPath);
        }

        /// <summary>
        /// Gets the content of the script at a given path as plain text.
        /// </summary>
        /// <returns>Returns the content of the script at the given path as plain text.</returns>
        /// <inheritdoc cref="GetScriptRef(string)"/>
        public static string GetScriptContent(string scriptPath)
        {
            return GetScriptRef(scriptPath, out ScriptRef scriptRef) ? scriptRef.ScriptContent : null;
        }

        /// <param name="scriptContent">Outputs the content of the script at the given path.</param>
        /// <inheritdoc cref="GetScriptContent(string)"/>
        public static bool GetScriptContent(string scriptPath, out string scriptContent)
        {
            scriptContent = GetScriptContent(scriptPath);
            return !string.IsNullOrEmpty(scriptContent);
        }

        /// <summary>
        /// Gets all the types from scripts in the project.
        /// </summary>
        /// <returns>Returns all the main types declared in <see cref="MonoScript"/> assets.</returns>
        public static Type[] GetProjectTypes()
        {
            return GetScriptRefsList().Map(i => i.GetScriptType());
        }

        /// <summary>
        /// Create <see cref="ScriptRef"/> instances for every type in the project.
        /// </summary>
        private static List<ScriptRef> GetScriptRefsList()
        {
            // Cancel if all the types have already been processed
            if (s_scriptRefs != null)
                return s_scriptRefs;

            s_scriptRefs = new List<ScriptRef>();

            // Convert excluded directories to absolute paths
            string[] excludedDirectories = new string[CoreEditorConfig.I.ExcludedDirectories.Length];
            for (int i = 0; i < excludedDirectories.Length; i++)
                excludedDirectories[i] = CoreEditorConfig.I.ExcludedDirectories[i].ToAbsolutePath();

            // For each script in the project
            foreach (MonoScript scriptAsset in ObjectUtility.FindAssets<MonoScript>())
            {
                string scriptAssetPath = AssetDatabase.GetAssetPath(scriptAsset).ToAbsolutePath();
                // Skip if the current script asset is in an excluded directory
                if (IsInExcludedDirectory(scriptAssetPath))
                    continue;

                ScriptRef scriptRef = new ScriptRef(scriptAssetPath);
                // Skip if the type can't be extracted from the script asset
                if (!scriptRef.GetScriptType(out Type type))
                    continue;

                // Skip if the script type is declared in an excluded assembly
                if (IsFromExcludedAssembly(type))
                    continue;

                s_scriptRefs.Add(scriptRef);
            }

            return s_scriptRefs;

            // Checks if a given path leads to an excluded directory
            bool IsInExcludedDirectory(string absolutePath)
            {
                // Cancel if no excluded directory defined
                if (excludedDirectories.Length <= 0)
                    return false;

                foreach (string excludedPath in excludedDirectories)
                {
                    // Skip if the current script asset is in an excluded directory
                    if (absolutePath.StartsWith(excludedPath))
                        return true;
                }
                return false;
            }

            // Checks if a given type is declared in an excluded assembly
            bool IsFromExcludedAssembly(Type type)
            {
                // Cancel if no excluded assembly defined
                if (CoreEditorConfig.I.ExcludedAssembliesNames.Length <= 0)
                    return false;

                string assemblyName = type.Assembly.GetName().Name;
                foreach (string excludedAssembly in CoreEditorConfig.I.ExcludedAssembliesNames)
                {
                    if (assemblyName == excludedAssembly)
                        return true;
                }
                return false;
            }
        }

        #endregion

    }

}
