using System;
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
    /// Tests for <see cref="EditorConfig{T}"/> and <see cref="EditorConfigUtility"/>.
    /// </summary>
    /// <remarks>
    /// All tests use <see cref="EEditorConfigScope.Editor"/>-scoped configs, which are backed by <see cref="EditorPrefs"/> rather than
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

        /// <summary>
        /// Sets up a known-empty state before each test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ResetState();
        }

        /// <summary>
        /// Removes everything the test may have loaded or stored.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            ResetState();
        }

        /// <summary>
        /// Unloads the test configs and removes their EditorPrefs entries, so each test loads from a known-empty state.
        /// </summary>
        private static void ResetState()
        {
            EditorConfigUtility.Unload(typeof(EditorScopeTestConfig));
            EditorConfigUtility.Unload(typeof(CustomKeyTestConfig));
            EditorConfigUtility.Unload(typeof(LegacyTestConfig));

            EditorPrefs.DeleteKey(GetEditorPrefsKey(typeof(EditorScopeTestConfig).FullName));
            EditorPrefs.DeleteKey(GetEditorPrefsKey(CustomKeyTestConfig.CustomKey));
            EditorPrefs.DeleteKey(GetEditorPrefsKey(typeof(LegacyTestConfig).FullName));

            EditorScopeTestConfig.LoadedCount = 0;
            LegacyTestConfig.PostLoadCount = 0;
        }

        /// <summary>
        /// Recomputes the EditorPrefs key the utility uses for an <see cref="EEditorConfigScope.Editor"/>-scoped config.
        /// </summary>
        /// <param name="key">The key of the config (its type's full name if not defined explicitly).</param>
        /// <returns>Returns the computed EditorPrefs key.</returns>
        private static string GetEditorPrefsKey(string key)
        {
            return $"{EditorPrefsKeyPrefix}{key}";
        }

        #region Instance

        /// <summary>
        /// Checks that a fresh instance holds its declared defaults when nothing is stored.
        /// </summary>
        [Test]
        public void Instance_NoSavedData_ReturnsInstanceWithDefaults()
        {
            EditorScopeTestConfig config = EditorScopeTestConfig.Instance;

            Assert.IsNotNull(config, "A fresh instance should be created even when nothing is stored.");
            Assert.AreEqual(0, config.value, "With no saved data, the loaded config should hold its declared defaults.");
        }

        /// <summary>
        /// Checks that the instance is cached after the first access.
        /// </summary>
        [Test]
        public void Instance_SecondCall_ReturnsSameCachedInstance()
        {
            EditorScopeTestConfig first = EditorScopeTestConfig.Instance;
            EditorScopeTestConfig second = EditorScopeTestConfig.I;

            Assert.AreSame(first, second, "Instance should return the already-loaded instance from its cache, not reload a new one.");
        }

        /// <summary>
        /// Checks that the instance can't be unloaded by Unity on scene changes.
        /// </summary>
        [Test]
        public void Instance_UsesDontSaveHideFlags()
        {
            EditorScopeTestConfig config = EditorScopeTestConfig.Instance;

            Assert.AreEqual(HideFlags.DontSave, config.hideFlags & HideFlags.DontSave, "Loaded configs should use HideFlags.DontSave.");
            Assert.AreEqual(HideFlags.None, config.hideFlags & HideFlags.NotEditable, "Loaded configs should stay editable.");
        }

        /// <summary>
        /// Checks that a type without <see cref="EditorConfigAttribute"/> can't be loaded.
        /// </summary>
        [Test]
        public void Instance_TypeWithoutEditorConfigAttribute_LogsErrorAndReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("doesn't use " + nameof(EditorConfigAttribute)));
            NoAttributeTestConfig config = NoAttributeTestConfig.Instance;

            Assert.IsNull(config, "Loading a config whose type lacks [EditorConfig] should fail.");
        }

        /// <summary>
        /// Checks that a saved value is read back after the config is unloaded.
        /// </summary>
        [Test]
        public void Instance_AfterSaveAndUnload_ReloadsPersistedValue()
        {
            EditorScopeTestConfig config = EditorScopeTestConfig.Instance;
            config.value = 123;
            Assert.IsTrue(config.Save());

            EditorConfigUtility.Unload(typeof(EditorScopeTestConfig));
            EditorScopeTestConfig reloaded = EditorScopeTestConfig.Instance;

            Assert.AreNotSame(config, reloaded, "Unloading should force a brand-new instance to be loaded.");
            Assert.AreEqual(123, reloaded.value, "The reloaded config should reflect the persisted value.");
        }

        #endregion


        #region Save

        /// <summary>
        /// Checks that saving an <see cref="EEditorConfigScope.Editor"/>-scoped config writes to EditorPrefs.
        /// </summary>
        [Test]
        public void Save_EditorScope_WritesSerializedJsonToEditorPrefs()
        {
            EditorScopeTestConfig config = EditorScopeTestConfig.Instance;
            config.value = 42;

            bool result = config.Save();

            Assert.IsTrue(result, "Saving an Editor-scoped config should succeed.");
            string key = GetEditorPrefsKey(typeof(EditorScopeTestConfig).FullName);
            Assert.IsTrue(EditorPrefs.HasKey(key), "The config JSON should be stored under the computed EditorPrefs key.");
            StringAssert.Contains("42", EditorPrefs.GetString(key), "The serialized JSON should contain the saved field value.");
        }

        /// <summary>
        /// Checks that <see cref="EditorConfigAttribute.Key"/> replaces the type name in the storage location.
        /// </summary>
        [Test]
        public void Save_CustomKey_WritesUnderCustomKey()
        {
            CustomKeyTestConfig config = CustomKeyTestConfig.Instance;
            config.value = 7;

            Assert.IsTrue(config.Save());

            Assert.IsTrue(EditorPrefs.HasKey(GetEditorPrefsKey(CustomKeyTestConfig.CustomKey)), "The config should use its custom key.");
            Assert.IsFalse(EditorPrefs.HasKey(GetEditorPrefsKey(typeof(CustomKeyTestConfig).FullName)), "The type name should be ignored.");
        }

        /// <summary>
        /// Checks that a type without <see cref="EditorConfigAttribute"/> can't be saved.
        /// </summary>
        [Test]
        public void Save_TypeWithoutEditorConfigAttribute_LogsErrorAndReturnsFalse()
        {
            NoAttributeTestConfig config = ScriptableObject.CreateInstance<NoAttributeTestConfig>();
            try
            {
                LogAssert.Expect(LogType.Error, new Regex("doesn't use " + nameof(EditorConfigAttribute)));
                bool result = config.Save();

                Assert.IsFalse(result, "Saving a config whose type lacks [EditorConfig] should fail.");
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        #endregion


        #region OnLoaded

        /// <summary>
        /// Checks that the load hook runs once when the config is loaded.
        /// </summary>
        [Test]
        public void Instance_OnLoad_InvokesOnLoadedOnce()
        {
            Assert.AreEqual(0, EditorScopeTestConfig.LoadedCount, "Precondition: OnLoaded counter should be reset before loading.");

            _ = EditorScopeTestConfig.Instance;

            Assert.AreEqual(1, EditorScopeTestConfig.LoadedCount, "OnLoaded should be invoked exactly once when the config is loaded.");
        }

        /// <summary>
        /// Checks that the load hook doesn't run again for a cached instance.
        /// </summary>
        [Test]
        public void Instance_CachedInstance_DoesNotInvokeOnLoadedAgain()
        {
            _ = EditorScopeTestConfig.Instance;
            Assert.AreEqual(1, EditorScopeTestConfig.LoadedCount);

            _ = EditorScopeTestConfig.Instance;

            Assert.AreEqual(1, EditorScopeTestConfig.LoadedCount, "OnLoaded should not run again for a cached instance.");
        }

        /// <summary>
        /// Checks that the load hook can use the instance getter without loading the config again.
        /// </summary>
        [Test]
        public void Instance_OnLoadedUsesInstance_ReturnsLoadedInstance()
        {
            EditorScopeTestConfig config = EditorScopeTestConfig.Instance;

            Assert.AreSame(config, config.InstanceFromOnLoaded, "Instance should return the config being loaded when used from OnLoaded.");
        }

        #endregion


        #region Unload

        /// <summary>
        /// Checks that unloading a config destroys its instance.
        /// </summary>
        [Test]
        public void Unload_LoadedConfig_DestroysInstance()
        {
            EditorScopeTestConfig config = EditorScopeTestConfig.Instance;

            EditorConfigUtility.Unload(typeof(EditorScopeTestConfig));

            Assert.IsTrue(config == null, "The unloaded instance should be destroyed.");
        }

        #endregion


        #region Legacy API

#pragma warning disable CS0618 // The legacy API is obsolete, but must keep working until it's removed

        /// <summary>
        /// Checks that the obsolete <see cref="EditorConfigUtility.Get{TConfig}(out TConfig)"/> still loads configs and calls
        /// <see cref="IEditorConfig.PostLoad"/>.
        /// </summary>
        [Test]
        public void LegacyGet_OnLoad_ReturnsInstanceAndInvokesPostLoadOnce()
        {
            Assert.IsTrue(EditorConfigUtility.Get(out LegacyTestConfig first));
            Assert.IsTrue(EditorConfigUtility.Get(out LegacyTestConfig second));

            Assert.IsNotNull(first);
            Assert.AreSame(first, second, "Get should return the already-loaded instance from its cache.");
            Assert.AreEqual(1, LegacyTestConfig.PostLoadCount, "PostLoad should be invoked exactly once when the config is loaded.");
        }

        /// <summary>
        /// Checks that the obsolete <see cref="EditorConfigUtility.Save(IEditorConfig)"/> still persists configs.
        /// </summary>
        [Test]
        public void LegacySave_AfterUnload_ReloadsPersistedValue()
        {
            LegacyTestConfig config = EditorConfigUtility.Get<LegacyTestConfig>();
            config.value = 99;
            Assert.IsTrue(EditorConfigUtility.Save(config));

            EditorConfigUtility.Unload(typeof(LegacyTestConfig));
            LegacyTestConfig reloaded = EditorConfigUtility.Get<LegacyTestConfig>();

            Assert.AreEqual(99, reloaded.value, "The reloaded config should reflect the persisted value.");
        }

#pragma warning restore CS0618

        #endregion

    }

    /// <summary>
    /// Editor-scoped (EditorPrefs-backed) config used to exercise <see cref="EditorConfig{T}"/> without touching the filesystem.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Editor)]
    internal class EditorScopeTestConfig : EditorConfig<EditorScopeTestConfig>
    {

        public int value = 0;

        /// <summary>
        /// Incremented each time <see cref="OnLoaded"/> runs, so tests can assert the load hook fires (and only once per load).
        /// </summary>
        public static int LoadedCount = 0;

        /// <summary>
        /// The value of <see cref="EditorConfig{T}.Instance"/> when read from <see cref="OnLoaded"/>.
        /// </summary>
        [NonSerialized]
        public EditorScopeTestConfig InstanceFromOnLoaded = null;

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            LoadedCount++;
            InstanceFromOnLoaded = Instance;
        }

    }

    /// <summary>
    /// Editor-scoped config that defines an explicit <see cref="EditorConfigAttribute.Key"/>.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Editor, Key = CustomKey)]
    internal class CustomKeyTestConfig : EditorConfig<CustomKeyTestConfig>
    {

        /// <summary>
        /// The custom key used to store this config.
        /// </summary>
        public const string CustomKey = "SideXP.Core.Tests.CustomKeyTestConfig_Custom";

        public int value = 0;

    }

    /// <summary>
    /// Config type that intentionally omits the <see cref="EditorConfigAttribute"/>, used to exercise the save/load failure paths.
    /// </summary>
    internal class NoAttributeTestConfig : EditorConfig<NoAttributeTestConfig> { }

#pragma warning disable CS0618 // Implements the obsolete interface on purpose, to test the legacy API

    /// <summary>
    /// Editor-scoped config using the obsolete <see cref="IEditorConfig"/> interface.
    /// </summary>
    [EditorConfig(EEditorConfigScope.Editor)]
    internal class LegacyTestConfig : ScriptableObject, IEditorConfig
    {

        public int value = 0;

        /// <summary>
        /// Incremented each time <see cref="PostLoad"/> runs.
        /// </summary>
        public static int PostLoadCount = 0;

        /// <inheritdoc/>
        public void PostLoad()
        {
            PostLoadCount++;
        }

    }

#pragma warning restore CS0618

}
