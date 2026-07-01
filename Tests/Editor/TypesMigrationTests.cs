using System;
using System.Collections;
using System.IO;
using System.Reflection;

using NUnit.Framework;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="TypesMigration"/>.
    /// </summary>
    /// <remarks>
    /// Only the safe branches are exercised. The core migration path — registering a type and resolving a renamed one via its previous
    /// names — is not unit-testable in-package: <see cref="TypesMigration.Resolve(string)"/> only registers a type whose script
    /// <see cref="ScriptUtility.GetScriptPath(Type, out string)"/> can resolve, which hits the default <c>Packages</c> exclusion (so
    /// in-package/test types never register); a real rename needs a recompile (domain reload); and the registration path writes
    /// <c>ProjectSettings/…TypesMigration.json</c>. As a safety net, the config file is captured and restored around every test, and the
    /// loaded-config cache is cleared, so nothing this test does can persist.
    /// </remarks>
    public class TypesMigrationTests
    {

        private string _configPath;
        private byte[] _originalConfigBytes;

        [SetUp]
        public void SetUp()
        {
            _configPath = PathEditorUtility.ProjectSettingsPath + "/" + typeof(TypesMigration).FullName + ".json";
            _originalConfigBytes = File.Exists(_configPath) ? File.ReadAllBytes(_configPath) : null;
            ClearLoadedConfigs();
        }

        [TearDown]
        public void TearDown()
        {
            ClearLoadedConfigs();

            // Restore the config file to its pre-test state, in case a Resolve happened to register + save a type.
            if (_originalConfigBytes != null)
                File.WriteAllBytes(_configPath, _originalConfigBytes);
            else if (File.Exists(_configPath))
                File.Delete(_configPath);
        }

        private static void ClearLoadedConfigs()
        {
            FieldInfo field = typeof(EditorConfigUtility).GetField("s_loadedConfigs", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
                return;
            ((IList)field.GetValue(null)).Clear();
        }

        #region Resolve

        [Test]
        public void Resolve_NullTypeName_ReturnsNull()
        {
            Assert.IsNull(TypesMigration.Resolve((string)null));
        }

        [Test]
        public void Resolve_EmptyTypeName_ReturnsNull()
        {
            Assert.IsNull(TypesMigration.Resolve(string.Empty));
        }

        [Test]
        public void Resolve_UnknownTypeName_ReturnsNull()
        {
            Assert.IsNull(TypesMigration.Resolve("Some.Unknown.Type_9f3a, NoSuchAssembly"));
        }

        [Test]
        public void Resolve_OutOverload_UnknownTypeName_ReturnsFalse()
        {
            bool result = TypesMigration.Resolve("Some.Unknown.Type_9f3a, NoSuchAssembly", out Type type);
            Assert.IsFalse(result);
            Assert.IsNull(type);
        }

        [Test]
        public void Resolve_Type_ReturnsAssemblyQualifiedName()
        {
            // Resolve(Type) echoes the AQN whether or not the type ends up tracked (an in-package type won't, due to the Packages exclusion).
            string result = TypesMigration.Resolve(typeof(SampleScriptableObject));
            Assert.AreEqual(typeof(SampleScriptableObject).AssemblyQualifiedName, result);
        }

        [Test]
        public void Resolve_NullType_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => TypesMigration.Resolve((Type)null));
        }

        #endregion


        #region Reload

        [Test]
        public void Reload_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => TypesMigration.Reload());
        }

        #endregion

    }

}
