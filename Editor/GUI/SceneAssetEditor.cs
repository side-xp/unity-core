using UnityEngine;
using UnityEditor;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Custom editor for <see cref="SceneAsset"/>.
    /// </summary>
    [CustomEditor(typeof(SceneAsset), isFallback = true)]
    public class SceneAssetEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SceneAsset sceneAsset = target as SceneAsset;

            using (new EnabledScope())
            {
                if (!sceneAsset.IsIncluded())
                {
                    EditorGUILayout.HelpBox("This scene is not included in Build Settings.", MessageType.Warning);
                    if (GUILayout.Button("Add Scene to Build Settings", GUI.skin.button.Bold().FontSizeDiff(3), MoreGUI.HeightLOpt))
                        sceneAsset.AddToBuildSettings();
                }
                else if (!sceneAsset.IsEnabled())
                {
                    EditorGUILayout.HelpBox("This scene is disabled in Build Settings.", MessageType.Warning);
                    if (GUILayout.Button("Enable Scene in Build Settings", GUI.skin.button.Bold().FontSizeDiff(3), MoreGUI.HeightLOpt))
                        sceneAsset.Enable();
                }
                else
                {
                    EditorGUILayout.HelpBox("This scene is included and enabled in Build Settings.", MessageType.Info);
                    if (GUILayout.Button("Disable Scene in Build Settings"))
                        sceneAsset.Disable();
                }
            }
        }

    }

}