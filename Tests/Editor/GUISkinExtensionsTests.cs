using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="GUISkinExtensions"/>.
    /// </summary>
    public class GUISkinExtensionsTests
    {

        private GUISkin _skin;

        [SetUp]
        public void SetUp()
        {
            _skin = ScriptableObject.CreateInstance<GUISkin>();
            // Register a named custom style so FindStyle can resolve it by name.
            _skin.customStyles = new[] { new GUIStyle { name = "MyStyle" } };
        }

        [TearDown]
        public void TearDown()
        {
            if (_skin != null)
                Object.DestroyImmediate(_skin);
        }

        [Test]
        public void FindStyle_ExistingStyle_ReturnsThatStyleNotFallback()
        {
            var fallback = new GUIStyle { name = "Fallback" };

            GUIStyle result = _skin.FindStyle("MyStyle", fallback);

            Assert.IsNotNull(result);
            Assert.AreEqual("MyStyle", result.name);
            Assert.AreNotSame(fallback, result);
        }

        [Test]
        public void FindStyle_MissingStyle_ReturnsFallback()
        {
            var fallback = new GUIStyle { name = "Fallback" };

            GUIStyle result = _skin.FindStyle("DoesNotExist", fallback);

            Assert.AreSame(fallback, result);
        }

        [Test]
        public void FindStyle_MissingStyle_ReturnsNullFallbackWhenGiven()
        {
            Assert.IsNull(_skin.FindStyle("DoesNotExist", null));
        }

    }

}
