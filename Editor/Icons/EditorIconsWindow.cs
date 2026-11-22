using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Window that lists the available editor icons, so their names can be found and copied.
    /// </summary>
    /// <remarks>
    /// Icons are listed by pages, so only the icons of the current page are downloaded and drawn. Clicking an icon copies its name.
    /// </remarks>
    internal class EditorIconsWindow : EditorWindow
    {

        #region Fields

        /// <summary>
        /// Title of the window.
        /// </summary>
        private const string WindowTitle = "Icons List";

        /// <summary>
        /// Path of the menu that opens the window.
        /// </summary>
        private const string MenuItem = IconsMenu.MenuPath + "/" + WindowTitle;

        /// <summary>
        /// Name of the icon of the window tab.
        /// </summary>
        private const string WindowIcon = "icons";

        /// <summary>
        /// Number of icons per page. Enough to browse quickly, few enough to download and draw a page in a moment. Divisible by 2, 3, 4,
        /// 5 and 6, so the last row of a full page is complete for most window widths.
        /// </summary>
        private const int IconsPerPage = 40;

        /// <summary>
        /// Minimum width of an icon badge. Badges are stretched to fill the width of the window.
        /// </summary>
        private const float MinBadgeWidth = 180f;

        /// <summary>
        /// Height of an icon badge.
        /// </summary>
        private const float BadgeHeight = 40f;

        /// <summary>
        /// Space between badges, and around the grid of badges.
        /// </summary>
        private const float BadgeSpacing = 4f;

        /// <summary>
        /// Space between the border of a badge and its content, and between its icon and its texts.
        /// </summary>
        private const float BadgePadding = 8f;

        /// <summary>
        /// Height of the category line under the icon name in a badge.
        /// </summary>
        private const float CategoryLineHeight = 14f;

        /// <summary>
        /// Width of the category dropdown in the toolbar.
        /// </summary>
        private const float CategoryPopupWidth = 160f;

        /// <summary>
        /// Space before the controls of the toolbar that are not toolbar buttons, so they don't touch the previous control.
        /// </summary>
        private const float ToolbarSpacing = 6f;

        /// <summary>
        /// Width of the pagination controls in the toolbar.
        /// </summary>
        private const float PaginationWidth = 220f;

        /// <summary>
        /// Minimum time between two measures of the downloaded icons, in seconds, since a measure lists all the stored files.
        /// </summary>
        private const double MeasureInterval = 5;

        /// <summary>
        /// Label of the dropdown option that shows the icons of every category.
        /// </summary>
        private const string AllCategoriesLabel = "All categories";

        /// <summary>
        /// The search text.
        /// </summary>
        [SerializeField]
        private string _search = string.Empty;

        /// <summary>
        /// The category of the listed icons, or empty for all categories.
        /// </summary>
        [SerializeField]
        private string _category = string.Empty;

        /// <summary>
        /// Whether only the icons with a filled style are listed, in that style.
        /// </summary>
        [SerializeField]
        private bool _isFilledOnly = false;

        /// <summary>
        /// The index of the current page.
        /// </summary>
        [SerializeField]
        private int _page = 0;

        /// <summary>
        /// The scroll position of the grid of icons.
        /// </summary>
        [SerializeField]
        private Vector2 _scrollPosition = Vector2.zero;

        /// <summary>
        /// The search field of the toolbar.
        /// </summary>
        private SearchField _searchField = null;

        /// <summary>
        /// The list of icons the filtered icons come from, to filter again when the index changes.
        /// </summary>
        private IReadOnlyList<IconInfo> _filterSource = null;

        /// <summary>
        /// The icons that match the search, category and style filter.
        /// </summary>
        private List<IconInfo> _filteredIcons = new List<IconInfo>();

        /// <summary>
        /// Whether the filter changed since the icons have been filtered.
        /// </summary>
        private bool _isFilterDirty = true;

        /// <summary>
        /// The number of downloaded icon files, at the last measure.
        /// </summary>
        private int _downloadedFileCount = 0;

        /// <summary>
        /// The total size of the downloaded icon files at the last measure, in bytes.
        /// </summary>
        private long _downloadedByteCount = 0;

        /// <summary>
        /// The time of the last measure of the downloaded icons.
        /// </summary>
        private double _lastMeasureTime = double.MinValue;

        #endregion


        #region Lifecycle

        /// <summary>
        /// Opens the editor icons window.
        /// </summary>
        /// <returns>Returns the opened window.</returns>
        [MenuItem(MenuItem, priority = 0)]
        public static EditorIconsWindow Open()
        {
            EditorIconsWindow window = GetWindow<EditorIconsWindow>(false, WindowTitle, true);
            window.Show();
            return window;
        }

        /// <summary>
        /// Called when the window is loaded.
        /// </summary>
        private void OnEnable()
        {
            _searchField = new SearchField();
            _isFilterDirty = true;
        }

        /// <summary>
        /// Called when the window gets focus.
        /// </summary>
        private void OnFocus()
        {
            // The icons may have been downloaded or deleted while the window was not focused
            _lastMeasureTime = double.MinValue;
        }

        #endregion


        #region UI

        /// <summary>
        /// Draws the window.
        /// </summary>
        private void OnGUI()
        {
            titleContent = EditorIcons.IconContent(WindowIcon, WindowTitle);
            bool isIndexAvailable = EditorIcons.IsIndexAvailable;
            if (isIndexAvailable)
                UpdateFilter();

            DrawToolbar();

            if (isIndexAvailable)
                DrawIcons();
            else
                DrawIndexState();

            DrawStatusBar();
        }

        /// <summary>
        /// Draws the toolbar: search field, category dropdown, filled filter and pagination.
        /// </summary>
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();

                _search = _searchField.OnToolbarGUI(_search);

                string[] categories = EditorIcons.Categories;
                var options = new string[categories.Length + 1];
                options[0] = AllCategoriesLabel;
                Array.Copy(categories, 0, options, 1, categories.Length);
                int selectedOption = Array.IndexOf(categories, _category) + 1;
                GUILayoutOption categoryWidth = GUILayout.Width(CategoryPopupWidth);
                selectedOption = EditorGUILayout.Popup(selectedOption, options, EditorStyles.toolbarPopup, categoryWidth);
                _category = selectedOption > 0 ? categories[selectedOption - 1] : string.Empty;

                GUILayout.Space(ToolbarSpacing);
                var filledLabel = new GUIContent("Filled", "Only list the icons that exist in the filled style, in that style.");
                _isFilledOnly = GUILayout.Toggle(_isFilledOnly, filledLabel, EditorStyles.toggle, GUILayout.ExpandWidth(false));

                if (EditorGUI.EndChangeCheck())
                {
                    _isFilterDirty = true;
                    SetPage(0);
                }

                GUILayout.FlexibleSpace();

                var pagination = new Pagination(_page, IconsPerPage, _filteredIcons.Count);
                float paginationHeight = EditorGUIUtility.singleLineHeight;
                Rect paginationRect = GUILayoutUtility.GetRect(PaginationWidth, paginationHeight, GUILayout.Width(PaginationWidth));
                MoreEditorGUI.PaginationField(paginationRect, ref pagination);
                if (pagination.Page != _page)
                    SetPage(pagination.Page);
            }
        }

        /// <summary>
        /// Draws the icons of the current page, as a grid of badges.
        /// </summary>
        private void DrawIcons()
        {
            if (_filteredIcons.Count == 0)
            {
                EditorGUILayout.HelpBox("No icon matches the search.", MessageType.Info);
                GUILayout.FlexibleSpace();
                return;
            }

            var pagination = new Pagination(_page, IconsPerPage, _filteredIcons.Count);
            EIconStyle style = _isFilledOnly ? EIconStyle.Filled : EIconStyle.Outline;

            // Columns are computed from the window width, minus the vertical scroll bar the grid may need
            float availableWidth = position.width - (BadgeSpacing * 2) - GUI.skin.verticalScrollbar.fixedWidth;
            int columnCount = Mathf.Max(1, Mathf.FloorToInt((availableWidth + BadgeSpacing) / (MinBadgeWidth + BadgeSpacing)));
            float badgeWidth = (availableWidth - (BadgeSpacing * (columnCount - 1))) / columnCount;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUILayout.Space(BadgeSpacing);
            for (int rowStart = pagination.FirstIndex; rowStart < pagination.LastIndex; rowStart += columnCount)
            {
                Rect row = GUILayoutUtility.GetRect(availableWidth, BadgeHeight, GUILayout.ExpandWidth(true));
                for (int column = 0; column < columnCount && rowStart + column < pagination.LastIndex; column++)
                {
                    var badgeRect = new Rect(row.x + BadgeSpacing + (column * (badgeWidth + BadgeSpacing)), row.y, badgeWidth, BadgeHeight);
                    DrawBadge(badgeRect, _filteredIcons[rowStart + column], style);
                }
                GUILayout.Space(BadgeSpacing);
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draws the badge of an icon: the icon on the left, its name and category on the right. Clicking it copies the icon name.
        /// </summary>
        /// <param name="position">The position of the badge.</param>
        /// <param name="icon">The icon to draw.</param>
        /// <param name="style">The style in which to draw the icon.</param>
        private void DrawBadge(Rect position, IconInfo icon, EIconStyle style)
        {
            string tags = icon.Tags.Count > 0 ? string.Join(", ", icon.Tags) : "none";
            var buttonContent = new GUIContent(string.Empty, $"{icon.Name}\nTags: {tags}\n\nClick to copy the name.");
            if (GUI.Button(position, buttonContent))
            {
                EditorGUIUtility.systemCopyBuffer = icon.Name;
                ShowNotification(new GUIContent($"Copied \"{icon.Name}\""));
            }
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            float iconSize = EditorIcons.GetPointSize(EIconSize.Medium);
            var iconRect = new Rect(position.x + BadgePadding, position.y + ((position.height - iconSize) / 2), iconSize, iconSize);
            GUI.DrawTexture(iconRect, EditorIcons.GetIcon(icon.Name, style, EIconColor.Auto, EIconSize.Medium), ScaleMode.ScaleToFit);

            float textX = iconRect.xMax + BadgePadding;
            float textHeight = EditorGUIUtility.singleLineHeight + CategoryLineHeight;
            var nameRect = new Rect(
                textX,
                position.y + ((position.height - textHeight) / 2),
                position.xMax - textX - BadgePadding,
                EditorGUIUtility.singleLineHeight
            );
            GUI.Label(nameRect, icon.Name, EditorStyles.boldLabel);

            var categoryRect = new Rect(nameRect.x, nameRect.yMax, nameRect.width, CategoryLineHeight);
            GUI.Label(categoryRect, icon.Category, EditorStyles.miniLabel);
        }

        /// <summary>
        /// Draws the state of the index while it's not available.
        /// </summary>
        private void DrawIndexState()
        {
            string message = IconIndexRefresher.IsRefreshing
                ? "Downloading the icons index..."
                : "The icons index is not available. Check your network connection, then click \"Refresh index\".";
            EditorGUILayout.HelpBox(message, MessageType.Info);
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// Draws the status bar: index version, number of listed icons, size of the downloaded icons, and actions.
        /// </summary>
        private void DrawStatusBar()
        {
            MeasureDownloadedIcons();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                string version = IconIndexStore.TryGetIndex(out IconIndex index) ? $"Tabler {index.version}" : "No index";
                string downloaded = $"{IconsMenu.FormatSize(_downloadedByteCount)} downloaded ({_downloadedFileCount} files)";
                GUILayout.Label($"{version}  |  {_filteredIcons.Count} icons  |  {downloaded}", EditorStyles.miniLabel);

                GUILayout.FlexibleSpace();

                string folder = IconStorage.RootFolder;
                using (new EditorGUI.DisabledScope(!Directory.Exists(folder)))
                using (new IconSizeScope(EIconSize.Small))
                {
                    GUIContent openContent = EditorIcons.IconContent("folder-open", "Open folder", $"Open the icons folder:\n{folder}");
                    if (GUILayout.Button(openContent, EditorStyles.toolbarButton))
                        OpenIconsFolder(folder);
                }

                using (new EditorGUI.DisabledScope(IconIndexRefresher.IsRefreshing))
                using (new IconSizeScope(EIconSize.Small))
                {
                    GUIContent refreshContent = EditorIcons.IconContent(
                        "refresh",
                        "Refresh index",
                        "Download the latest version of the icons index."
                    );
                    if (GUILayout.Button(refreshContent, EditorStyles.toolbarButton))
                        IconIndexRefresher.Refresh(true, _ => OnIconsChanged());

                    GUIContent deleteContent = EditorIcons.IconContent(
                        "trash",
                        "Delete downloaded icons",
                        "Delete the downloaded icons, so they're downloaded again when needed."
                    );
                    if (GUILayout.Button(deleteContent, EditorStyles.toolbarButton))
                    {
                        IconsMenu.DeleteDownloadedIcons();
                        OnIconsChanged();
                        // The dialogs break the current layout
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        #endregion


        #region Private API

        /// <summary>
        /// Filters the icons again if the filter or the index changed.
        /// </summary>
        private void UpdateFilter()
        {
            IReadOnlyList<IconInfo> icons = EditorIcons.Icons;
            if (!_isFilterDirty && ReferenceEquals(icons, _filterSource))
                return;

            _filterSource = icons;
            _filteredIcons = IconFilter.Filter(icons, _search, _category, _isFilledOnly);
            _isFilterDirty = false;
        }

        /// <summary>
        /// Opens the icons folder in the file explorer of the OS.
        /// </summary>
        /// <remarks>
        /// <see cref="EditorUtility.RevealInFinder(string)"/> opens the folder that contains the given path, so the index file is
        /// revealed when it exists, which opens the icons folder itself. Otherwise, the icons folder is revealed in its parent.
        /// </remarks>
        /// <param name="folder">The absolute path to the icons folder.</param>
        private static void OpenIconsFolder(string folder)
        {
            string indexPath = IconStorage.IndexFilePath;
            EditorUtility.RevealInFinder(File.Exists(indexPath) ? indexPath : folder);
        }

        /// <summary>
        /// Goes to a page, and scrolls back to its top.
        /// </summary>
        /// <param name="page">The index of the page.</param>
        private void SetPage(int page)
        {
            _page = page;
            _scrollPosition = Vector2.zero;
        }

        /// <summary>
        /// Measures the downloaded icons, unless they've been measured recently.
        /// </summary>
        private void MeasureDownloadedIcons()
        {
            double time = EditorApplication.timeSinceStartup;
            if (time - _lastMeasureTime < MeasureInterval)
                return;

            _lastMeasureTime = time;
            try
            {
                IconStorage.MeasureDownloadedIcons(out _downloadedFileCount, out _downloadedByteCount);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                _downloadedFileCount = 0;
                _downloadedByteCount = 0;
            }
        }

        /// <summary>
        /// Called when the index or the downloaded icons changed from this window: measures and filters again.
        /// </summary>
        private void OnIconsChanged()
        {
            _lastMeasureTime = double.MinValue;
            _isFilterDirty = true;
            Repaint();
        }

        #endregion

    }

}
