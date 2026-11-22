using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Draws <see cref="CorePreferences"/> in the Preferences window: the default inspector, followed by the sections that need custom
    /// actions.
    /// </summary>
    [CustomEditor(typeof(CorePreferences))]
    internal class CorePreferencesEditor : Editor
    {

        /// <summary>
        /// Properties not drawn by the default inspector: the script field, and the ones drawn in a custom section.
        /// </summary>
        private static readonly string[] ExcludedProperties = { "m_Script", CorePreferences.IconsFolderPropertyName };

        /// <inheritdoc cref="Editor.OnInspectorGUI"/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, ExcludedProperties);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            IconsPreferencesGUI.DrawLayout((CorePreferences)target, serializedObject.FindProperty(CorePreferences.IconsFolderPropertyName));
        }

    }

}
