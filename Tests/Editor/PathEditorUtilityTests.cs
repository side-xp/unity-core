using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    public class PathEditorUtilityTests
    {

        [Test]
        public void ProjectSettingsPath_IsAbsolute_UnderProject()
        {
            Assert.IsTrue(System.IO.Path.IsPathRooted(PathEditorUtility.ProjectSettingsPath));
            Assert.IsTrue(PathEditorUtility.ProjectSettingsPath.StartsWith(PathUtility.ProjectPath));
            Assert.IsTrue(PathEditorUtility.ProjectSettingsPath.EndsWith("ProjectSettings"));
        }

        [Test]
        public void UserSettingsPath_IsAbsolute_UnderProject()
        {
            Assert.IsTrue(System.IO.Path.IsPathRooted(PathEditorUtility.UserSettingsPath));
            Assert.IsTrue(PathEditorUtility.UserSettingsPath.StartsWith(PathUtility.ProjectPath));
            Assert.IsTrue(PathEditorUtility.UserSettingsPath.EndsWith("UserSettings"));
        }

    }

}
