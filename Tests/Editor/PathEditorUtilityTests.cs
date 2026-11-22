using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="PathEditorUtility"/>.
    /// </summary>
    public class PathEditorUtilityTests
    {

        /// <summary>
        /// Makes a function that reads environment variables from a dictionary instead of the real environment.
        /// </summary>
        /// <param name="variables">The environment variables, as name and value pairs.</param>
        /// <returns>Returns the function to pass to <see cref="PathEditorUtility.ResolveUserCachePath"/>.</returns>
        private static Func<string, string> MakeEnvironment(params (string name, string value)[] variables)
        {
            var dictionary = new Dictionary<string, string>();
            foreach ((string name, string value) in variables)
                dictionary[name] = value;

            return name => dictionary.TryGetValue(name, out string value) ? value : null;
        }

        /// <summary>
        /// Gets a fake local application data folder, rooted on the current OS so the tests also work from a macOS or Linux editor.
        /// </summary>
        /// <returns>Returns the path to the fake folder, using the separators of the current OS.</returns>
        private static string GetFakeLocalAppData()
        {
            return Path.Combine(Path.GetPathRoot(Path.GetTempPath()), "Users", "someone", "AppData", "Local");
        }

        #region Project paths

        /// <summary>
        /// The /ProjectSettings folder is an absolute path in the project folder.
        /// </summary>
        [Test]
        public void ProjectSettingsPath_IsAbsolute_UnderProject()
        {
            Assert.IsTrue(Path.IsPathRooted(PathEditorUtility.ProjectSettingsPath));
            Assert.IsTrue(PathEditorUtility.ProjectSettingsPath.StartsWith(PathUtility.ProjectPath));
            Assert.IsTrue(PathEditorUtility.ProjectSettingsPath.EndsWith("ProjectSettings"));
        }

        /// <summary>
        /// The /UserSettings folder is an absolute path in the project folder.
        /// </summary>
        [Test]
        public void UserSettingsPath_IsAbsolute_UnderProject()
        {
            Assert.IsTrue(Path.IsPathRooted(PathEditorUtility.UserSettingsPath));
            Assert.IsTrue(PathEditorUtility.UserSettingsPath.StartsWith(PathUtility.ProjectPath));
            Assert.IsTrue(PathEditorUtility.UserSettingsPath.EndsWith("UserSettings"));
        }

        #endregion


        #region User cache

        /// <summary>
        /// The user cache folder of the running editor is an absolute path, without trailing separator.
        /// </summary>
        [Test]
        public void UserCachePath_IsAbsolute_WithoutTrailingSeparator()
        {
            Assert.IsTrue(Path.IsPathRooted(PathEditorUtility.UserCachePath));
            StringAssert.DoesNotEndWith("/", PathEditorUtility.UserCachePath);
        }

        /// <summary>
        /// On Windows, the user cache folder is the local application data folder, with backslashes replaced by slashes.
        /// </summary>
        [Test]
        public void ResolveUserCachePath_Windows_UsesLocalAppData()
        {
            string localAppData = GetFakeLocalAppData();
            Func<string, string> environment = MakeEnvironment(("LOCALAPPDATA", localAppData));

            string cachePath = PathEditorUtility.ResolveUserCachePath(RuntimePlatform.WindowsEditor, environment);

            Assert.AreEqual(localAppData.Replace('\\', '/'), cachePath);
            StringAssert.DoesNotContain("\\", cachePath);
        }

        /// <summary>
        /// On Windows, a trailing separator in the local application data folder is removed.
        /// </summary>
        [Test]
        public void ResolveUserCachePath_WindowsWithTrailingSeparator_RemovesSeparator()
        {
            string localAppData = GetFakeLocalAppData();
            Func<string, string> environment = MakeEnvironment(("LOCALAPPDATA", $"{localAppData}\\"));

            string cachePath = PathEditorUtility.ResolveUserCachePath(RuntimePlatform.WindowsEditor, environment);

            Assert.AreEqual(localAppData.Replace('\\', '/'), cachePath);
        }

        /// <summary>
        /// On macOS, the user cache folder is the user's caches folder.
        /// </summary>
        [Test]
        public void ResolveUserCachePath_MacOS_UsesLibraryCaches()
        {
            string home = "/Users/someone";
            Func<string, string> environment = MakeEnvironment(("HOME", home));

            string cachePath = PathEditorUtility.ResolveUserCachePath(RuntimePlatform.OSXEditor, environment);

            Assert.AreEqual($"{home}/Library/Caches", cachePath);
        }

        /// <summary>
        /// On Linux, the user cache folder is the folder defined by XDG_CACHE_HOME, when defined.
        /// </summary>
        [Test]
        public void ResolveUserCachePath_LinuxWithXdgCacheHome_UsesXdgCacheHome()
        {
            string xdgCacheHome = "/home/someone/custom-cache";
            Func<string, string> environment = MakeEnvironment(("XDG_CACHE_HOME", xdgCacheHome), ("HOME", "/home/someone"));

            string cachePath = PathEditorUtility.ResolveUserCachePath(RuntimePlatform.LinuxEditor, environment);

            Assert.AreEqual(xdgCacheHome, cachePath);
        }

        /// <summary>
        /// On Linux, the user cache folder is ~/.cache when XDG_CACHE_HOME is not defined.
        /// </summary>
        [Test]
        public void ResolveUserCachePath_LinuxWithoutXdgCacheHome_UsesDotCache()
        {
            string home = "/home/someone";
            Func<string, string> environment = MakeEnvironment(("HOME", home));

            string cachePath = PathEditorUtility.ResolveUserCachePath(RuntimePlatform.LinuxEditor, environment);

            Assert.AreEqual($"{home}/.cache", cachePath);
        }

        /// <summary>
        /// On Linux, a relative XDG_CACHE_HOME is ignored, as required by the XDG Base Directory specification.
        /// </summary>
        [Test]
        public void ResolveUserCachePath_LinuxWithRelativeXdgCacheHome_UsesDotCache()
        {
            string home = "/home/someone";
            Func<string, string> environment = MakeEnvironment(("XDG_CACHE_HOME", "relative/cache"), ("HOME", home));

            string cachePath = PathEditorUtility.ResolveUserCachePath(RuntimePlatform.LinuxEditor, environment);

            Assert.AreEqual($"{home}/.cache", cachePath);
        }

        #endregion

    }

}
