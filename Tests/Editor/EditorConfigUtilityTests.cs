using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="EditorConfigUtility"/>.
    /// </summary>
    /// <remarks>
    /// All tests use an <see cref="EEditorConfigScope.Editor"/>-scoped config, which is backed by <see cref="EditorPrefs"/> rather than
    /// a file, so the round-trip stays entirely off the filesystem (no <c>ProjectSettings</c>/<c>UserSettings</c> writes) and cleans up
    /// with a single <see cref="EditorPrefs.DeleteKey(string)"/>. We never call <see cref="EditorConfigUtility.SaveAll"/> here: it would
    /// flush every loaded config, including the editor's real <c>CoreEditorConfig</c>, to disk.
    /// </remarks>
    public class EditorConfigUtilityTests
    {

        /// <summary>
        /// Mirrors the private <c>EditorConfigUtility.EditorPrefsKeyPrefix</c>.
        /// </summary>
        private const string EditorPrefsKeyPrefix = "EditorConfigs_";

        private readonly List<Object> _toDestroy = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            ResetState();
        }

        [TearDown]
        public void TearDown()
        {
            ResetState();
            foreach (Object obj in _toDestroy)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _toDestroy.Clear();
        }

        /// <summary>
        /// Clears the utility's static load cache and removes the EditorPrefs entry used by the test config, so each test loads from a
        /// known-empty state.
        /// </summary>
        private void ResetState()
        {
            ClearLoadedConfigs();
            EditorPrefs.DeleteKey(GetEditorPrefsKey(typeof(EditorScopeTestConfig)));
            EditorScopeTestConfig.PostLoadCount = 0;
        }

        /// <summary>
        /// Empties <c>EditorConfigUtility.s_loadedConfigs</c> via reflection, forcing the next <see cref="EditorConfigUtility.Get{T}()"/>
        /// to reload from disk/prefs instead of returning a cached instance.
        /// </summary>
        private static void ClearLoadedConfigs()
        {
            FieldInfo field = typeof(EditorConfigUtility).GetField("s_loadedConfigs", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "The private s_loadedConfigs cache field was not found (renamed?).");
            IList list = (IList)field.GetValue(null);
            list.Clear();
        }

        /// <summary>
        /// Recomputes the EditorPrefs key the utility uses for an <see cref="EEditorConfigScope.Editor"/>-scoped config, mirroring
        /// <c>GetConfigFilePath</c>.
        /// </summary>
        private static string GetEditorPrefsKey(Type configType)
        {
            return $"{EditorPrefsKeyPrefix}{configType}/{configType.FullName}.json";
        }

        #region Save

        [Test]
        public void Save_TypeWithoutEditorConfigAttribute_LogsErrorAndReturnsFalse()
        {
            NoAttributeTestConfig config = ScriptableObject.CreateInstance<NoAttributeTestConfig>();
            _toDestroy.Add(config);

            LogAssert.Expect(LogType.Error, new Regex("doesn't use " + nameof(EditorConfigAttribute)));
            bool result = EditorConfigUtility.Save(config);

            Assert.IsFalse(result, "Saving a config whose type lacks [EditorConfig] should fail.");
        }

        [Test]
        public void Save_EditorScope_WritesSerializedJsonToEditorPrefs()
        {
            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig config));
            _toDestroy.Add(config);
            config.value = 42;

            bool result = EditorConfigUtility.Save(config);

            Assert.IsTrue(result, "Saving an Editor-scoped config should succeed.");
            string key = GetEditorPrefsKey(typeof(EditorScopeTestConfig));
            Assert.IsTrue(EditorPrefs.HasKey(key), "The config JSON should be stored under the computed EditorPrefs key.");
            StringAssert.Contains("42", EditorPrefs.GetString(key), "The serialized JSON should contain the saved field value.");
        }

        #endregion


        #region Get

        [Test]
        public void Get_TypeWithoutEditorConfigAttribute_LogsErrorAndReturnsFalse()
        {
            LogAssert.Expect(LogType.Error, new Regex("doesn't use " + nameof(EditorConfigAttribute)));
            bool result = EditorConfigUtility.Get(out NoAttributeTestConfig config);

            Assert.IsFalse(result, "Loading a config whose type lacks [EditorConfig] should fail.");
            Assert.IsNull(config, "The out instance should be the default (null) on failure.");
        }

        [Test]
        public void Get_NoSavedData_ReturnsInstanceWithDefaults()
        {
            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig config));
            _toDestroy.Add(config);

            Assert.IsNotNull(config, "A fresh instance should be created even when nothing is stored.");
            Assert.AreEqual(0, config.value, "With no saved data, the loaded config should hold its declared defaults.");
        }

        [Test]
        public void Get_SecondCall_ReturnsSameCachedInstance()
        {
            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig first));
            _toDestroy.Add(first);
            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig second));

            Assert.AreSame(first, second, "Get should return the already-loaded instance from its cache, not reload a new one.");
        }

        [Test]
        public void Get_AfterSaveAndCacheCleared_ReloadsPersistedValue()
        {
            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig config));
            _toDestroy.Add(config);
            config.value = 123;
            Assert.IsTrue(EditorConfigUtility.Save(config));

            // Drop the cached instance so the next Get reloads from EditorPrefs rather than returning the in-memory one.
            ClearLoadedConfigs();

            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig reloaded));
            _toDestroy.Add(reloaded);

            Assert.AreNotSame(config, reloaded, "Clearing the cache should force a brand-new instance to be loaded.");
            Assert.AreEqual(123, reloaded.value, "The reloaded config should reflect the persisted value.");
        }

        #endregion


        #region PostLoad

        [Test]
        public void Get_OnLoad_InvokesPostLoadOnce()
        {
            Assert.AreEqual(0, EditorScopeTestConfig.PostLoadCount, "Precondition: PostLoad counter should be reset before loading.");

            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig config));
            _toDestroy.Add(config);

            Assert.AreEqual(1, EditorScopeTestConfig.PostLoadCount, "PostLoad should be invoked exactly once when the config is loaded.");
        }

        [Test]
        public void Get_CachedInstance_DoesNotInvokePostLoadAgain()
        {
            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig config));
            _toDestroy.Add(config);
            Assert.AreEqual(1, EditorScopeTestConfig.PostLoadCount);

            // A second Get hits the cache and must not re-run PostLoad.
            Assert.IsTrue(EditorConfigUtility.Get(out EditorScopeTestConfig _));

            Assert.AreEqual(1, EditorScopeTestConfig.PostLoadCount, "PostLoad should not run again for a cached instance.");
        }

        #endregion

    }

    /// <summary>
    /// Editor-scoped (EditorPrefs-backed) config used to exercise <see cref="EditorConfigUtility"/> without touching the filesystem.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Editor)]
    internal class EditorScopeTestConfig : ScriptableObject, IEditorConfig
    {

        public int value = 0;

        /// <summary>
        /// Incremented each time <see cref="PostLoad"/> runs, so tests can assert the load hook fires (and only once per load).
        /// </summary>
        public static int PostLoadCount = 0;

        public void PostLoad()
        {
            PostLoadCount++;
        }

    }

    /// <summary>
    /// Config type that intentionally omits the <see cref="EditorConfigAttribute"/>, used to exercise the save/load failure paths.
    /// </summary>
    internal class NoAttributeTestConfig : ScriptableObject, IEditorConfig
    {

        public void PostLoad() { }

    }

}
