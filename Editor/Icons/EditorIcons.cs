using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Gets editor icons from the Tabler icon set, downloaded on demand and shared by every project on this machine.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Icons are downloaded the first time they're requested. Until then, a loading placeholder of the same size is returned,
    /// so layouts don't jump when the icon arrives, and the editor views are repainted once it's there.</item>
    /// <item>The functions of this class never return null, and never throw: unknown icon names log a warning once, and return an
    /// empty placeholder.</item>
    /// <item>The returned textures are owned by this class: callers must not destroy them.</item>
    /// <item>This class is meant to be used from the main thread only.</item>
    /// </list>
    /// Icon names are the Tabler ones (see <see href="https://tabler.io/icons"/>), as <c>home</c> or <c>arrow-left</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// GUIContent content = new GUIContent("Home", EditorIcons.GetIcon("home"));
    /// Texture2D icon = EditorIcons.GetIcon("star", EIconStyle.Filled, EIconColor.White, EIconSize.Large);
    /// </code>
    /// </example>
    public static class EditorIcons
    {

        #region Fields

        /// <summary>
        /// Minimum time between two checks of the index file, in seconds. The file may be refreshed by another editor, but checking
        /// it on each request would touch the file system for each icon drawn.
        /// </summary>
        private const double IndexCheckInterval = 1;

        /// <summary>
        /// Minimum time between two automatic downloads of the index after a failure, in seconds.
        /// </summary>
        private const double AutoRefreshRetryDelay = 60;

        /// <summary>
        /// URL of the Tabler icons catalog, mentioned in warnings.
        /// </summary>
        private const string CatalogWebsite = "https://tabler.io/icons";

        /// <summary>
        /// The index in use, or null if none is available.
        /// </summary>
        private static IconIndex s_index = null;

        /// <summary>
        /// The entries of <see cref="s_index"/>, by icon name.
        /// </summary>
        private static Dictionary<string, IconIndexEntry> s_entries = new Dictionary<string, IconIndexEntry>();

        /// <summary>
        /// The categories of <see cref="s_index"/>, sorted by name.
        /// </summary>
        private static string[] s_categories = Array.Empty<string>();

        /// <summary>
        /// The informations about the icons of <see cref="s_index"/>.
        /// </summary>
        private static IReadOnlyList<IconInfo> s_icons = Array.Empty<IconInfo>();

        /// <summary>
        /// The last time the index file has been checked.
        /// </summary>
        private static double s_lastIndexCheckTime = double.MinValue;

        /// <summary>
        /// The last time the index has been downloaded automatically.
        /// </summary>
        private static double s_lastAutoRefreshTime = double.MinValue;

        /// <summary>
        /// The names for which a warning has been logged, so it's logged only once.
        /// </summary>
        private static HashSet<string> s_warnedNames = new HashSet<string>();

        /// <summary>
        /// The names requested before the index was available, to check once it's loaded.
        /// </summary>
        private static HashSet<string> s_namesToCheck = new HashSet<string>();

        #endregion


        #region Public API

        /// <summary>
        /// Gets an icon, or a placeholder of the same size if it's being downloaded or doesn't exist.
        /// </summary>
        /// <remarks>
        /// If the icon is not downloaded yet, the download starts, and a loading placeholder is returned. Call this function again (as
        /// on the next repaint) to get the icon once it's downloaded.
        /// </remarks>
        /// <param name="name">The name of the icon, as <c>home</c>.</param>
        /// <param name="style">
        /// The style of the icon. <see cref="EIconStyle.Filled"/> falls back to <see cref="EIconStyle.Outline"/> if the icon has no
        /// filled version.
        /// </param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="size">The size of the icon on screen. The texture size depends on it and on the display scale.</param>
        /// <returns>Returns the icon texture, or a placeholder of the same size. Never returns null.</returns>
        public static Texture2D GetIcon(string name, EIconStyle style = EIconStyle.Outline, EIconColor color = EIconColor.Auto,
            EIconSize size = EIconSize.Small)
        {
            return GetIconOrPlaceholder(name, style, color, IconSizes.GetPointSize(size), out _);
        }

        /// <summary>
        /// Gets an icon only if it's already available.
        /// </summary>
        /// <remarks>
        /// If the icon is not downloaded yet, the download starts as with
        /// <see cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)"/>, so a later call may succeed.
        /// </remarks>
        /// <param name="name">The name of the icon, as <c>home</c>.</param>
        /// <param name="icon">Outputs the icon texture, or null if it's not available yet.</param>
        /// <param name="style"><inheritdoc cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)" path="/param[@name='style']"/></param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="size"><inheritdoc cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)" path="/param[@name='size']"/></param>
        /// <returns>Returns true if the icon is available.</returns>
        public static bool TryGetIcon(string name, out Texture2D icon, EIconStyle style = EIconStyle.Outline,
            EIconColor color = EIconColor.Auto, EIconSize size = EIconSize.Small)
        {
            Texture2D texture = GetIconOrPlaceholder(name, style, color, IconSizes.GetPointSize(size), out bool isAvailable);
            icon = isAvailable ? texture : null;
            return isAvailable;
        }

        /// <summary>
        /// Gets an icon asynchronously, downloading it if needed.
        /// </summary>
        /// <remarks>
        /// The callback is called on the main thread: right away if the icon is already available, or once it's downloaded. It's not
        /// called if the icon doesn't exist or can't be downloaded.
        /// </remarks>
        /// <param name="name">The name of the icon, as <c>home</c>.</param>
        /// <param name="onLoaded">Called with the icon texture once it's available.</param>
        /// <param name="style"><inheritdoc cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)" path="/param[@name='style']"/></param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="size"><inheritdoc cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)" path="/param[@name='size']"/></param>
        /// <exception cref="ArgumentNullException">Thrown if the callback is null.</exception>
        public static void LoadIcon(string name, Action<Texture2D> onLoaded, EIconStyle style = EIconStyle.Outline,
            EIconColor color = EIconColor.Auto, EIconSize size = EIconSize.Small)
        {
            if (onLoaded == null)
                throw new ArgumentNullException(nameof(onLoaded));

            LoadIcon(name, onLoaded, style, color, IconSizes.GetPointSize(size));
        }

        /// <summary>
        /// Checks if an icon exists in the index, in the given style.
        /// </summary>
        /// <remarks>
        /// Before the index is available, names can't be checked, and this function returns false.
        /// </remarks>
        /// <param name="name">The name of the icon, as <c>home</c>.</param>
        /// <param name="style">The style to check.</param>
        /// <returns>Returns true if the icon exists in the given style.</returns>
        public static bool Exists(string name, EIconStyle style = EIconStyle.Outline)
        {
            if (string.IsNullOrEmpty(name) || !TryGetIndex())
                return false;

            return s_entries.TryGetValue(name, out IconIndexEntry entry) && (style == EIconStyle.Outline || entry.hasFilled);
        }

        /// <summary>
        /// Gets the size on screen of an icon, in points.
        /// </summary>
        /// <param name="size">The size of the icon.</param>
        /// <returns>Returns the size on screen, in points.</returns>
        public static float GetPointSize(EIconSize size)
        {
            return IconSizes.GetPointSize(size);
        }

        /// <summary>
        /// Checks if the index has been downloaded and loaded.
        /// </summary>
        /// <remarks>
        /// If it's not available, its download starts.
        /// </remarks>
        public static bool IsIndexAvailable => TryGetIndex();

        /// <summary>
        /// Downloads the latest version of the index, and cleans up the downloaded icons that don't match it anymore.
        /// </summary>
        /// <remarks>
        /// The refresh runs in the background, and its failures are logged. To show its progress, use the
        /// <c>Tools/Sideways Experiments/Icons/Refresh Icons Index</c> menu instead.
        /// </remarks>
        public static void RefreshIndex()
        {
            IconIndexRefresher.Refresh(false, OnIndexRefreshed);
        }

        /// <summary>
        /// Gets the names of all the categories in the index, sorted by name.
        /// </summary>
        /// <remarks>
        /// Empty while the index is not available.
        /// </remarks>
        public static string[] Categories => TryGetIndex() ? (string[])s_categories.Clone() : Array.Empty<string>();

        /// <summary>
        /// Gets the informations about all the icons in the index.
        /// </summary>
        /// <remarks>
        /// Empty while the index is not available.
        /// </remarks>
        public static IReadOnlyList<IconInfo> Icons => TryGetIndex() ? s_icons : Array.Empty<IconInfo>();

        #endregion


        #region IMGUI

        /// <summary>
        /// Makes a <see cref="GUIContent"/> with an icon, for IMGUI controls.
        /// </summary>
        /// <remarks>
        /// IMGUI draws the image of a <see cref="GUIContent"/> at its size in pixels, so draw the content in an
        /// <see cref="IconSizeScope"/> to keep the icon at its size on screen at high display scales. Make the content again on each
        /// draw (it's cheap): the image is a placeholder until the icon is downloaded.
        /// </remarks>
        /// <param name="name">The name of the icon, as <c>home</c>.</param>
        /// <param name="text">The text of the content, or null for an icon only.</param>
        /// <param name="tooltip">The tooltip of the content, or null for none.</param>
        /// <param name="style"><inheritdoc cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)" path="/param[@name='style']"/></param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="size">The size of the icon on screen.</param>
        /// <returns>Returns the created content.</returns>
        public static GUIContent IconContent(string name, string text = null, string tooltip = null, EIconStyle style = EIconStyle.Outline,
            EIconColor color = EIconColor.Auto, EIconSize size = EIconSize.Small)
        {
            return new GUIContent(text, GetIcon(name, style, color, size), tooltip);
        }

        /// <summary>
        /// Draws an IMGUI button with an icon, at its size on screen.
        /// </summary>
        /// <param name="position">The position of the button.</param>
        /// <param name="name">The name of the icon, as <c>home</c>.</param>
        /// <param name="label">
        /// The text and tooltip of the button, or null for an icon only. Its image is replaced by the icon, without changing it.
        /// </param>
        /// <param name="style"><inheritdoc cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)" path="/param[@name='style']"/></param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="size">The size of the icon on screen.</param>
        /// <returns>Returns true if the button has been clicked.</returns>
        public static bool IconButton(Rect position, string name, GUIContent label = null, EIconStyle style = EIconStyle.Outline,
            EIconColor color = EIconColor.Auto, EIconSize size = EIconSize.Small)
        {
            GUIContent content = MakeButtonContent(name, label, style, color, size);
            using (new IconSizeScope(size))
                return GUI.Button(position, content);
        }

        /// <summary>
        /// Draws an IMGUI button with an icon, at its size on screen, using automatic layout.
        /// </summary>
        /// <param name="name">The name of the icon, as <c>home</c>.</param>
        /// <param name="label">
        /// The text and tooltip of the button, or null for an icon only. Its image is replaced by the icon, without changing it.
        /// </param>
        /// <param name="style"><inheritdoc cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)" path="/param[@name='style']"/></param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="size">The size of the icon on screen.</param>
        /// <param name="options">The layout options of the button.</param>
        /// <returns>Returns true if the button has been clicked.</returns>
        public static bool IconButton(string name, GUIContent label = null, EIconStyle style = EIconStyle.Outline,
            EIconColor color = EIconColor.Auto, EIconSize size = EIconSize.Small, params GUILayoutOption[] options)
        {
            GUIContent content = MakeButtonContent(name, label, style, color, size);
            using (new IconSizeScope(size))
                return GUILayout.Button(content, options);
        }

        #endregion


        #region Internal API

        /// <summary>
        /// Whether the index is downloaded automatically when it's needed and not available. Meant to be disabled by tests.
        /// </summary>
        internal static bool IsAutoRefreshEnabled { get; set; } = true;

        /// <summary>
        /// Forgets the index in use and the warnings already logged, so the next request starts from a clean state. Meant for tests,
        /// along with <see cref="IconDownloads.Reset"/>.
        /// </summary>
        internal static void ResetState()
        {
            SetIndex(null);
            s_lastIndexCheckTime = double.MinValue;
            s_lastAutoRefreshTime = double.MinValue;
            s_warnedNames.Clear();
            s_namesToCheck.Clear();
        }

        /// <summary>
        /// Gets an icon drawn at any size on screen, or a placeholder of the same size if it's being downloaded or doesn't exist.
        /// </summary>
        /// <remarks>
        /// Same as <see cref="GetIcon(string, EIconStyle, EIconColor, EIconSize)"/>, for custom sizes (as set from USS on the
        /// <see cref="Icon"/> element).
        /// </remarks>
        /// <param name="name">The name of the icon.</param>
        /// <param name="style">The style of the icon.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="pointSize">The size of the icon on screen, in points.</param>
        /// <param name="isAvailable">Outputs true if the returned texture is the icon, or false if it's a placeholder.</param>
        /// <returns>Returns the icon texture, or a placeholder of the same size.</returns>
        internal static Texture2D GetIcon(string name, EIconStyle style, EIconColor color, float pointSize, out bool isAvailable)
        {
            return GetIconOrPlaceholder(name, style, color, pointSize, out isAvailable);
        }

        /// <summary>
        /// Gets an icon drawn at any size on screen asynchronously, downloading it if needed.
        /// </summary>
        /// <remarks>
        /// Same as <see cref="LoadIcon(string, Action{Texture2D}, EIconStyle, EIconColor, EIconSize)"/>, for custom sizes (as set from
        /// USS on the <see cref="Icon"/> element).
        /// </remarks>
        /// <param name="name">The name of the icon.</param>
        /// <param name="onLoaded">Called with the icon texture once it's available. Must not be null.</param>
        /// <param name="style">The style of the icon.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="pointSize">The size of the icon on screen, in points.</param>
        internal static void LoadIcon(string name, Action<Texture2D> onLoaded, EIconStyle style, EIconColor color, float pointSize)
        {
            Texture2D icon = GetIconOrPlaceholder(name, style, color, pointSize, out bool isAvailable);
            if (isAvailable)
            {
                onLoaded(icon);
                return;
            }

            if (!IconStorage.IsValidIconName(name))
                return;

            // Wait for the index if it's being downloaded (asking for it while refreshing joins the refresh in progress)
            if (s_index == null)
            {
                if (IconIndexRefresher.IsRefreshing)
                {
                    IconIndexRefresher.Refresh(false, job =>
                    {
                        if (job.IsSuccess)
                            LoadIcon(name, onLoaded, style, color, pointSize);
                    });
                }
                return;
            }

            if (!s_entries.TryGetValue(name, out IconIndexEntry entry))
                return;

            IconDownloads.Request(s_index.version, entry, style, isStored =>
            {
                if (isStored)
                    LoadIcon(name, onLoaded, style, color, pointSize);
            });
        }

        #endregion


        #region Private API

        /// <summary>
        /// Gets an icon, or a placeholder of the same size if it's being downloaded or doesn't exist.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="style">The style of the icon.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="pointSize">The size of the icon on screen, in points.</param>
        /// <param name="isAvailable">Outputs true if the returned texture is the icon, or false if it's a placeholder.</param>
        /// <returns>Returns the icon texture, or a placeholder of the same size.</returns>
        private static Texture2D GetIconOrPlaceholder(string name, EIconStyle style, EIconColor color, float pointSize,
            out bool isAvailable)
        {
            isAvailable = false;
            int pixelSize = IconSizes.GetPixelSize(pointSize, EditorGUIUtility.pixelsPerPoint);

            try
            {
                if (!IconStorage.IsValidIconName(name))
                {
                    WarnOnce(name, $"Invalid editor icon name \"{name}\": only lowercase letters, digits and hyphens are allowed.");
                    return IconTextures.GetEmptyPlaceholder(pixelSize);
                }

                if (!TryGetIndex())
                {
                    // The name will be checked once the index is loaded
                    s_namesToCheck.Add(name);
                    return IconIndexRefresher.IsRefreshing
                        ? IconTextures.GetLoadingPlaceholder(pixelSize, pointSize)
                        : IconTextures.GetEmptyPlaceholder(pixelSize);
                }

                if (!s_entries.TryGetValue(name, out IconIndexEntry entry))
                {
                    WarnUnknownName(name);
                    return IconTextures.GetEmptyPlaceholder(pixelSize);
                }

                if (!entry.hasFilled)
                    style = EIconStyle.Outline;

                bool isBlack = IconSizes.IsBlack(color, EditorGUIUtility.isProSkin);
                if (IconTextures.TryGet(name, entry.category, style, pixelSize, isBlack, out Texture2D texture))
                {
                    isAvailable = true;
                    return texture;
                }

                if (IconDownloads.IsGivenUp(s_index.version, name, style))
                    return IconTextures.GetEmptyPlaceholder(pixelSize);

                // Not requested again on each repaint while it's downloading
                if (!IconDownloads.IsPending(s_index.version, name, style))
                    IconDownloads.Request(s_index.version, entry, style, null);

                return IconTextures.GetLoadingPlaceholder(pixelSize, pointSize);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return IconTextures.GetEmptyPlaceholder(pixelSize);
            }
        }

        /// <summary>
        /// Gets the index in use, reloading it if the index file changed, or downloading it automatically if there's none.
        /// </summary>
        /// <returns>Returns true if the index is available.</returns>
        private static bool TryGetIndex()
        {
            double time = EditorApplication.timeSinceStartup;
            if (s_index == null || time - s_lastIndexCheckTime >= IndexCheckInterval)
            {
                s_lastIndexCheckTime = time;
                IconIndexStore.TryGetIndex(out IconIndex storedIndex);
                if (!ReferenceEquals(storedIndex, s_index))
                    SetIndex(storedIndex);
            }

            if (s_index != null)
                return true;

            RequestIndex(time);
            return false;
        }

        /// <summary>
        /// Starts downloading the index in the background, unless it's already being downloaded, or a download failed recently.
        /// </summary>
        /// <param name="time">The current time, in seconds.</param>
        private static void RequestIndex(double time)
        {
            if (!IsAutoRefreshEnabled || IconIndexRefresher.IsRefreshing || time - s_lastAutoRefreshTime < AutoRefreshRetryDelay)
                return;

            s_lastAutoRefreshTime = time;
            IconIndexRefresher.Refresh(false, OnIndexRefreshed);
        }

        /// <summary>
        /// Called when a refresh of the index started from this class is done.
        /// </summary>
        /// <param name="job">The completed refresh.</param>
        private static void OnIndexRefreshed(IconIndexRefresh job)
        {
            if (!job.IsSuccess)
                return;

            // Check the new index file right away, and draw the icons that were waiting for it
            s_lastIndexCheckTime = double.MinValue;
            InternalEditorUtility.RepaintAllViews();
        }

        /// <summary>
        /// Uses an index, and checks the names requested before it was available.
        /// </summary>
        /// <param name="index">The index to use, or null if none is available.</param>
        private static void SetIndex(IconIndex index)
        {
            s_index = index;
            s_entries.Clear();

            if (index == null)
            {
                s_categories = Array.Empty<string>();
                s_icons = Array.Empty<IconInfo>();
                return;
            }

            var categories = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var icons = new List<IconInfo>(index.icons.Length);
            foreach (IconIndexEntry entry in index.icons)
            {
                if (entry == null || string.IsNullOrEmpty(entry.name))
                    continue;

                s_entries[entry.name] = entry;
                icons.Add(new IconInfo(entry));
                if (!string.IsNullOrEmpty(entry.category))
                    categories.Add(entry.category);
            }

            s_categories = new string[categories.Count];
            categories.CopyTo(s_categories);
            s_icons = new ReadOnlyCollection<IconInfo>(icons);

            foreach (string name in s_namesToCheck)
            {
                if (!s_entries.ContainsKey(name))
                    WarnUnknownName(name);
            }
            s_namesToCheck.Clear();
        }


        /// <summary>
        /// Makes the content of an icon button, from an optional label.
        /// </summary>
        /// <param name="name">The name of the icon.</param>
        /// <param name="label">The text and tooltip of the button, or null for an icon only. It's copied, not changed.</param>
        /// <param name="style">The style of the icon.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="size">The size of the icon on screen.</param>
        /// <returns>Returns the content of the button.</returns>
        private static GUIContent MakeButtonContent(string name, GUIContent label, EIconStyle style, EIconColor color, EIconSize size)
        {
            Texture2D icon = GetIcon(name, style, color, size);
            return label != null
                ? new GUIContent(label.text, icon, label.tooltip)
                : new GUIContent(icon);
        }

        /// <summary>
        /// Logs a warning for a name that doesn't exist in the index, once per name.
        /// </summary>
        /// <param name="name">The unknown name.</param>
        private static void WarnUnknownName(string name)
        {
            WarnOnce(name, $"Unknown editor icon \"{name}\". See the available icons at {CatalogWebsite}.");
        }

        /// <summary>
        /// Logs a warning about an icon name, once per name.
        /// </summary>
        /// <param name="name">The icon name the warning is about.</param>
        /// <param name="message">The warning to log.</param>
        private static void WarnOnce(string name, string message)
        {
            if (s_warnedNames.Add(name ?? string.Empty))
                Debug.LogWarning(message);
        }

        #endregion

    }

}
