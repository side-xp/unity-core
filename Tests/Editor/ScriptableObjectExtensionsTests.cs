using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    public class ScriptableObjectExtensionsTests
    {

        private SampleScriptableObject _target;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<SampleScriptableObject>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_target != null)
                Object.DestroyImmediate(_target);
            _target = null;
        }

        #region GetScriptPath

        [Test]
        public void GetScriptPath_ResolvesSourceScriptFile()
        {
            string path = _target.GetScriptPath();
            Assert.IsFalse(string.IsNullOrEmpty(path), "the script path should resolve for a file-matched ScriptableObject");
            StringAssert.EndsWith("SampleScriptableObject.cs", path);
        }

        #endregion


        #region ResetToDefaults

        [Test]
        public void ResetToDefaults_RestoresDefaultFieldValues()
        {
            _target.number = 99;
            _target.text = "changed";

            _target.ResetToDefaults();

            Assert.AreEqual(3, _target.number);
            Assert.AreEqual("default", _target.text);
        }

        #endregion

    }

}
