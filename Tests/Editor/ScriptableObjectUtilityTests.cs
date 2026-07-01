using System;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="ScriptableObjectUtility"/>.
    /// </summary>
    public class ScriptableObjectUtilityTests
    {

        private readonly List<Object> _toDestroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _toDestroy)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _toDestroy.Clear();
        }

        private T New<T>() where T : ScriptableObject
        {
            T obj = ScriptableObject.CreateInstance<T>();
            _toDestroy.Add(obj);
            return obj;
        }

        #region GetScriptPath

        [Test]
        public void GetScriptPath_Generic_ResolvesScriptFile()
        {
            string path = ScriptableObjectUtility.GetScriptPath<SampleScriptableObject>();
            StringAssert.EndsWith("SampleScriptableObject.cs", path);
        }

        [Test]
        public void GetScriptPath_Type_ResolvesScriptFile()
        {
            string path = ScriptableObjectUtility.GetScriptPath(typeof(SampleScriptableObject));
            StringAssert.EndsWith("SampleScriptableObject.cs", path);
        }

        [Test]
        public void GetScriptPath_Instance_ResolvesScriptFile()
        {
            string path = ScriptableObjectUtility.GetScriptPath(New<SampleScriptableObject>());
            StringAssert.EndsWith("SampleScriptableObject.cs", path);
        }

        [Test]
        public void GetScriptPath_UnresolvableType_ReturnsNull()
        {
            // This type isn't file-matched (it lives in the test file), so no MonoScript resolves — the result should be null, not "".
            string path = ScriptableObjectUtility.GetScriptPath(typeof(ScriptableObjectUtilitySampleUnmatched));
            Assert.IsNull(path);
        }

        [Test]
        public void GetScriptPath_NullType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ScriptableObjectUtility.GetScriptPath((Type)null));
        }

        [Test]
        public void GetScriptPath_NullInstance_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ScriptableObjectUtility.GetScriptPath((ScriptableObject)null));
        }

        [Test]
        public void GetScriptPath_NonScriptableObjectType_Throws()
        {
            Assert.Throws<ArgumentException>(() => ScriptableObjectUtility.GetScriptPath(typeof(int)));
        }

        #endregion


        #region ResetToDefaults

        [Test]
        public void ResetToDefaults_RestoresDefaultFieldValues()
        {
            SampleScriptableObject obj = New<SampleScriptableObject>();
            obj.number = 99;
            obj.text = "changed";

            ScriptableObjectUtility.ResetToDefaults(obj);

            Assert.AreEqual(3, obj.number);
            Assert.AreEqual("default", obj.text);
        }

        [Test]
        public void ResetToDefaults_NullObject_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => ScriptableObjectUtility.ResetToDefaults(null));
        }

        #endregion

    }

    /// <summary>
    /// A <see cref="ScriptableObject"/> whose class name does not match the file it lives in, so no source script asset resolves for it.
    /// Used to exercise the "unresolvable → null" path of <see cref="ScriptableObjectUtility.GetScriptPath(Type)"/>.
    /// </summary>
    internal class ScriptableObjectUtilitySampleUnmatched : ScriptableObject { }

}
