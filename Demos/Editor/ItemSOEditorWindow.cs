using UnityEngine;
using UnityEditor;

namespace SideXP.Core.Demos.EditorOnly
{
    /// <summary>
    /// Illustrates the use of OpenAsset callbacks. This window can only be opened by double-clicking on an <see cref="ItemSO"/> asset.<br/>
    /// See <see cref="ItemSO"/> script to learn more about how the callback is triggered.<br/>
    /// This workflow allow you to have a custom OpenAsset behavior, without putting all the editor code inside the asset itself.
    /// </summary>
    /// <remarks>Note the use of <see cref="InitializeOnLoadAttribute"/>, required if you want thie window to have its static constructor
    /// called.</remarks>
    [InitializeOnLoad]
    public class ItemSOEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Item Config";

        [SerializeField]
        private ItemSO _inspectedItemAsset = null;
        private Editor _inspectedItemAssetEditor = null;

        // Subscribe to the OpenAsset callback
        static ItemSOEditorWindow()
        {
            RuntimeObjectUtility.AddOpenAssetHandler((asset, line) => asset is ItemSO itemAsset && Open(itemAsset));
        }

        // Load the incpected asset editor after reload
        private void OnEnable()
        {
            if (_inspectedItemAssetEditor == null && _inspectedItemAsset != null)
                _inspectedItemAssetEditor = Editor.CreateEditor(_inspectedItemAsset);
        }

        // Destroy the inspected item editor on close or before reload
        private void OnDisable()
        {
            if (_inspectedItemAssetEditor != null)
                DestroyImmediate(_inspectedItemAssetEditor);
        }

        // Called when an ItemSO asset is double-clicked
        public static ItemSOEditorWindow Open(ItemSO itemAsset)
        {
            ItemSOEditorWindow window = GetWindow<ItemSOEditorWindow>(false, WindowTitle, true);

            if (window._inspectedItemAssetEditor != null)
                DestroyImmediate(window._inspectedItemAssetEditor);

            window._inspectedItemAsset = itemAsset;
            window._inspectedItemAssetEditor = itemAsset != null ? Editor.CreateEditor(itemAsset) : null;

            window.Show();
            return window;
        }

        // Draw GUI
        private void OnGUI()
        {
            if (_inspectedItemAssetEditor != null)
                _inspectedItemAssetEditor.OnInspectorGUI();
        }
    }
}
