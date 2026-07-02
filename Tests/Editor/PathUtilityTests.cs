using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class PathUtilityTests
    {

        #region ToPath

        [Test]
        public void ToPath_NormalizesBackslashes()
        {
            Assert.AreEqual("Assets/Foo/Bar", PathUtility.ToPath("Assets\\Foo\\Bar"));
        }

        [Test]
        public void ToPath_KeepsForwardSlashes()
        {
            Assert.AreEqual("Assets/Foo/Bar", PathUtility.ToPath("Assets/Foo/Bar"));
        }

        [Test]
        public void ToPath_MixedSeparators_AreNormalized()
        {
            Assert.AreEqual("Assets/Foo/Bar", PathUtility.ToPath("Assets\\Foo/Bar"));
        }

        [Test]
        public void ToPath_CustomSeparator()
        {
            Assert.AreEqual("Assets\\Foo\\Bar", PathUtility.ToPath("Assets/Foo/Bar", separator: '\\'));
        }

        [Test]
        public void ToPath_TrimsWhitespace()
        {
            Assert.AreEqual("Assets/Foo", PathUtility.ToPath("  Assets/Foo  "));
        }

        [Test]
        public void ToPath_NullOrEmpty_ReturnedUnchanged()
        {
            Assert.IsNull(PathUtility.ToPath(null));
            Assert.AreEqual(string.Empty, PathUtility.ToPath(""));
        }

        #endregion


        #region ToAbsolutePath

        [Test]
        public void ToAbsolutePath_NullOrEmpty_ReturnsProjectPath()
        {
            Assert.AreEqual(PathUtility.ProjectPath, PathUtility.ToAbsolutePath(""));
            Assert.AreEqual(PathUtility.ProjectPath, PathUtility.ToAbsolutePath(null));
        }

        [Test]
        public void ToAbsolutePath_RelativePath_IsRootedUnderProject()
        {
            string abs = PathUtility.ToAbsolutePath("Assets/Foo");
            Assert.IsTrue(abs.StartsWith(PathUtility.ProjectPath), "Expected an absolute path under the project.");
            Assert.IsTrue(abs.EndsWith("Assets/Foo"), "Expected the relative part to be preserved.");
        }

        #endregion


        #region ToRelativePath

        [Test]
        public void ToRelativePath_Empty_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, PathUtility.ToRelativePath(""));
        }

        [Test]
        public void ToRelativePath_AlreadyRelative_ReturnedAsIs()
        {
            Assert.AreEqual("Assets/Foo", PathUtility.ToRelativePath("Assets/Foo"));
        }

        [Test]
        public void ToRelativePath_ProjectRoot_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, PathUtility.ToRelativePath(PathUtility.ProjectPath));
        }

        [Test]
        public void RoundTrip_AbsoluteThenRelative()
        {
            string abs = PathUtility.ToAbsolutePath("Assets/Foo");
            Assert.AreEqual("Assets/Foo", PathUtility.ToRelativePath(abs));
        }

        [Test]
        public void ToRelativePath_PrefixSiblingFolder_ReturnedAsIs()
        {
            // A sibling folder sharing the project's name prefix is outside the project, so its path
            // must be returned unchanged rather than having the prefix stripped.
            string sibling = PathUtility.ProjectPath + "Sibling/foo";
            Assert.AreEqual(sibling, PathUtility.ToRelativePath(sibling));
        }

        #endregion


        #region IsProjectPath

        [Test]
        public void IsProjectPath_Empty_ReturnsTrue()
        {
            Assert.IsTrue(PathUtility.IsProjectPath(""));
        }

        [Test]
        public void IsProjectPath_RelativePath_ReturnsTrue()
        {
            Assert.IsTrue(PathUtility.IsProjectPath("Assets/Foo"));
        }

        [Test]
        public void IsProjectPath_AbsoluteUnderProject_ReturnsTrue()
        {
            Assert.IsTrue(PathUtility.IsProjectPath(PathUtility.ToAbsolutePath("Assets/Foo")));
        }

        [Test]
        public void IsProjectPath_OutsideProject_ReturnsFalse()
        {
            // The project's parent directory is never inside the project.
            string parent = System.IO.Directory.GetParent(PathUtility.ProjectPath).FullName;
            Assert.IsFalse(PathUtility.IsProjectPath(parent));
        }

        [Test]
        public void IsProjectPath_PrefixSiblingFolder_IsNotProjectPath()
        {
            // A sibling folder that merely shares the project's name prefix is NOT inside the project.
            string sibling = PathUtility.ProjectPath + "Sibling/foo";
            Assert.IsFalse(PathUtility.IsProjectPath(sibling));
        }

        #endregion

    }

}
