using System;
using System.Collections;
using System.Reflection;

using NUnit.Framework;

using UnityEngine;
using UnityEditor;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for the internal <see cref="ScriptCache"/> (exposed to this assembly via <c>InternalsVisibleTo</c>).
    /// </summary>
    /// <remarks>
    /// A cache entry is only <c>IsValid</c> when both its path resolves to a real <see cref="MonoScript"/> asset and its type name
    /// resolves — so the fixtures use a real project script (<see cref="SampleScriptableObject"/>) and its type. All mutations stay
    /// in-memory: <see cref="EditorConfigUtility"/>'s loaded-config cache is cleared before and after each test (via reflection), so the
    /// User-scoped <see cref="ScriptCache"/> instance we mutate is dropped and never written back to <c>UserSettings</c>.
    /// </remarks>
    public class ScriptCacheTests
    {

        private SampleScriptableObject _sampleInstance;
        private MonoScript _sampleScript;
        private string _samplePath;
        private readonly Type _sampleType = typeof(SampleScriptableObject);

        [SetUp]
        public void SetUp()
        {
            ClearLoadedConfigs();

            _sampleInstance = ScriptableObject.CreateInstance<SampleScriptableObject>();
            _sampleScript = MonoScript.FromScriptableObject(_sampleInstance);
            _samplePath = AssetDatabase.GetAssetPath(_sampleScript);
            Assert.IsFalse(string.IsNullOrEmpty(_samplePath), "Precondition: the sample script asset path should resolve.");
        }

        [TearDown]
        public void TearDown()
        {
            // Drop the mutated (in-memory) ScriptCache so our entries are never persisted.
            ClearLoadedConfigs();

            if (_sampleInstance != null)
                Object.DestroyImmediate(_sampleInstance);
            _sampleInstance = null;
        }

        /// <summary>
        /// Empties <c>EditorConfigUtility.s_loadedConfigs</c> via reflection.
        /// </summary>
        private static void ClearLoadedConfigs()
        {
            FieldInfo field = typeof(EditorConfigUtility).GetField("s_loadedConfigs", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
                return;
            ((IList)field.GetValue(null)).Clear();
        }

        #region Get / Set

        [Test]
        public void Set_ThenGetByPath_ReturnsCachedTypeAndScript()
        {
            ScriptCache.Set(_samplePath, _sampleType);

            bool result = ScriptCache.Get(_samplePath, out Type type, out MonoScript script);

            Assert.IsTrue(result, "A freshly set, valid entry should be retrievable.");
            Assert.AreEqual(_sampleType, type);
            Assert.AreEqual(_sampleScript, script);
        }

        [Test]
        public void Get_UnknownPath_ReturnsFalse()
        {
            bool result = ScriptCache.Get("Assets/DoesNotExist_9f3a.cs", out Type type, out MonoScript script);

            Assert.IsFalse(result);
            Assert.IsNull(type);
            Assert.IsNull(script);
        }

        [Test]
        public void Set_ThenGetByMonoScript_ReturnsCachedPathAndType()
        {
            ScriptCache.Set(_samplePath, _sampleType);

            bool result = ScriptCache.Get(_sampleScript, out string path, out Type type);

            Assert.IsTrue(result);
            Assert.AreEqual(_sampleType, type);
            Assert.IsFalse(string.IsNullOrEmpty(path));
        }

        #endregion


        #region Deduplication

        [Test]
        public void Set_SamePathDifferentType_KeepsLatestType()
        {
            // Regression for the path-dedup fix: setting the same path twice must leave only the latest type, not a stale first entry.
            ScriptCache.Set(_samplePath, typeof(int));
            ScriptCache.Set(_samplePath, _sampleType);

            ScriptCache.Get(_samplePath, out Type type, out _);

            Assert.AreEqual(_sampleType, type, "The path should map to the most recently set type.");
        }

        [Test]
        public void Set_SameTypeDifferentPath_RemovesPreviousPathEntry()
        {
            ScriptCache.Set(_samplePath, _sampleType);
            Assert.IsTrue(ScriptCache.Get(_samplePath, out _, out _), "Precondition: the entry should exist before re-homing the type.");

            // Setting the same type at a different path drops the old path's entry (a type maps to a single path).
            ScriptCache.Set("Assets/Other_9f3a.cs", _sampleType);

            bool result = ScriptCache.Get(_samplePath, out _, out _);
            Assert.IsFalse(result, "The original path's entry should have been removed when its type was set at another path.");
        }

        #endregion

    }

}
