using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="ScriptUtility"/> — focused on the deterministic parsing surface (the regex patterns and the
    /// <see cref="ScriptUtility.TryGetDeclaredType(string, out Type)"/> family, plus a directly-constructed
    /// <see cref="ScriptUtility.ScriptRef"/>).
    /// </summary>
    /// <remarks>
    /// The project-scan methods (<c>GetScriptRef*</c>, <c>GetProjectTypes</c>) are intentionally not exercised: they scan every
    /// <see cref="UnityEditor.MonoScript"/> in the project and read <c>CoreEditorConfig</c>, whose default
    /// <c>ExcludedDirectories = { "Packages" }</c> filters out every package-embedded fixture (same wall as
    /// <c>MonoScriptExtensions</c>). Fixtures are declared as temp <c>*.cs</c> files in the OS temp directory (deleted in teardown)
    /// and resolve to types that actually exist in this test assembly, so the parser can find them via reflection.
    /// </remarks>
    public class ScriptUtilityTests
    {

        private const string SampleClassContent =
@"namespace SideXP.Core.Tests
{
    public class SampleScriptableObject
    {
    }
}";

        private readonly List<Object> _toDestroy = new List<Object>();
        private readonly List<string> _tempFiles = new List<string>();

        [TearDown]
        public void TearDown()
        {
            // Drop any config instances loaded by ScriptRef.GetScriptType (it touches the User-scoped ScriptCache), so our in-memory
            // cache mutations are never persisted to UserSettings.
            ClearLoadedConfigs();

            foreach (string path in _tempFiles)
            {
                try { if (File.Exists(path)) File.Delete(path); }
                catch { /* best-effort cleanup */ }
            }
            _tempFiles.Clear();

            foreach (Object obj in _toDestroy)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
            _toDestroy.Clear();
        }

        /// <summary>
        /// Writes a temp file with the given extension and content, tracked for deletion in teardown.
        /// </summary>
        private string CreateTempFile(string extension, string content)
        {
            string path = Path.Combine(Path.GetTempPath(), $"ScriptUtilityTest_{Guid.NewGuid():N}{extension}");
            File.WriteAllText(path, content);
            _tempFiles.Add(path);
            return path;
        }

        /// <summary>
        /// Empties <c>EditorConfigUtility.s_loadedConfigs</c> via reflection so any config loaded during a test (e.g. the ScriptCache)
        /// is dropped before it can be saved.
        /// </summary>
        private static void ClearLoadedConfigs()
        {
            FieldInfo field = typeof(EditorConfigUtility).GetField("s_loadedConfigs", BindingFlags.NonPublic | BindingFlags.Static);
            if (field == null)
                return;
            ((IList)field.GetValue(null)).Clear();
        }

        #region ClassDeclarationPattern

        [Test]
        public void ClassDeclarationPattern_Class_CapturesNameAndBase()
        {
            Match match = ScriptUtility.ClassDeclarationPattern.Match("public class Foo : Bar");
            Assert.IsTrue(match.Success);
            Assert.AreEqual("Foo", match.Groups["class"].Value);
            Assert.AreEqual("Bar", match.Groups["base"].Value);
        }

        [TestCase("internal struct Baz", "Baz")]
        [TestCase("public interface IThing", "IThing")]
        [TestCase("public enum EColor", "EColor")]
        public void ClassDeclarationPattern_StructInterfaceEnum_CapturesName(string declaration, string expectedName)
        {
            Match match = ScriptUtility.ClassDeclarationPattern.Match(declaration);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(expectedName, match.Groups["class"].Value);
        }

        [TestCase("public static class Utils", "Utils")]
        [TestCase("public sealed partial class Widget", "Widget")]
        [TestCase("public abstract class Base", "Base")]
        [TestCase("public readonly struct RoStruct", "RoStruct")]
        public void ClassDeclarationPattern_WithModifiers_CapturesName(string declaration, string expectedName)
        {
            Match match = ScriptUtility.ClassDeclarationPattern.Match(declaration);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(expectedName, match.Groups["class"].Value);
        }

        [TestCase("static class InternalUtils", "InternalUtils")]
        [TestCase("partial class Split", "Split")]
        public void ClassDeclarationPattern_NoAccessModifierButOtherModifier_CapturesName(string declaration, string expectedName)
        {
            // Previously required public/internal; now any modifier (e.g. static/partial) is enough.
            Match match = ScriptUtility.ClassDeclarationPattern.Match(declaration);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(expectedName, match.Groups["class"].Value);
        }

        [TestCase("public record Rec", "Rec")]
        [TestCase("public record class RecClass", "RecClass")]
        [TestCase("internal record struct RecStruct", "RecStruct")]
        public void ClassDeclarationPattern_Record_CapturesName(string declaration, string expectedName)
        {
            Match match = ScriptUtility.ClassDeclarationPattern.Match(declaration);
            Assert.IsTrue(match.Success);
            Assert.AreEqual(expectedName, match.Groups["class"].Value);
        }

        [Test]
        public void ClassDeclarationPattern_Generic_CapturesNameAndGenerics()
        {
            Match match = ScriptUtility.ClassDeclarationPattern.Match("public class Pair<TKey, TValue> : Bar");
            Assert.IsTrue(match.Success);
            Assert.AreEqual("Pair", match.Groups["class"].Value);
            Assert.IsTrue(match.Groups["generics"].Success);
            StringAssert.Contains("TKey", match.Groups["generics"].Value);
            Assert.AreEqual("Bar", match.Groups["base"].Value);
        }

        [Test]
        public void ClassDeclarationPattern_BareTypeInComment_DoesNotMatch()
        {
            // A modifier-less "class X" (e.g. inside a comment/prose) must not match, to avoid false positives.
            Match match = ScriptUtility.ClassDeclarationPattern.Match("// this class does something useful");
            Assert.IsFalse(match.Success);
        }

        #endregion


        #region NamespaceDeclarationPattern

        [Test]
        public void NamespaceDeclarationPattern_CapturesDottedNamespace()
        {
            Match match = ScriptUtility.NamespaceDeclarationPattern.Match("namespace SideXP.Core.Tests");
            Assert.IsTrue(match.Success);
            Assert.AreEqual("SideXP.Core.Tests", match.Groups["namespace"].Value);
        }

        #endregion


        #region TryGetDeclaredType (content)

        [TestCase(null)]
        [TestCase("")]
        public void TryGetDeclaredType_NullOrEmptyContent_ReturnsFalse(string content)
        {
            bool result = ScriptUtility.TryGetDeclaredType(content, out Type type);
            Assert.IsFalse(result);
            Assert.IsNull(type);
        }

        [Test]
        public void TryGetDeclaredType_NoTypeDeclaration_ReturnsFalse()
        {
            bool result = ScriptUtility.TryGetDeclaredType("using System;\n// nothing declared here", out Type type);
            Assert.IsFalse(result);
            Assert.IsNull(type);
        }

        [Test]
        public void TryGetDeclaredType_UnresolvableType_ReturnsFalse()
        {
            bool result = ScriptUtility.TryGetDeclaredType(
                "namespace SideXP.Core.Tests { public class ThisTypeDoesNotExist_9f3a { } }", out Type type);
            Assert.IsFalse(result, "A parseable but non-existent type should not resolve.");
            Assert.IsNull(type);
        }

        [Test]
        public void TryGetDeclaredType_ExistingClass_ResolvesType()
        {
            bool result = ScriptUtility.TryGetDeclaredType(SampleClassContent, out Type type);
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(SampleScriptableObject), type);
        }

        [Test]
        public void TryGetDeclaredType_ExistingStruct_ResolvesType()
        {
            bool result = ScriptUtility.TryGetDeclaredType(
                "namespace SideXP.Core.Tests { internal struct ScriptUtilityStructSample { } }", out Type type);
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(ScriptUtilityStructSample), type);
        }

        [Test]
        public void TryGetDeclaredType_ExistingGenericType_ResolvesOpenGenericDefinition()
        {
            // Pins the generic-arity fix: "Sample<T>" must resolve to the "`1" reflection name.
            bool result = ScriptUtility.TryGetDeclaredType(
                "namespace SideXP.Core.Tests { internal class ScriptUtilityGenericSample<T> { } }", out Type type);
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(ScriptUtilityGenericSample<>), type);
        }

        #endregion


        #region TryGetDeclaredType (TextAsset)

        [Test]
        public void TryGetDeclaredType_NullTextAsset_ReturnsFalse()
        {
            bool result = ScriptUtility.TryGetDeclaredType((TextAsset)null, out Type type);
            Assert.IsFalse(result);
            Assert.IsNull(type);
        }

        [Test]
        public void TryGetDeclaredType_TextAssetWithValidContent_ResolvesType()
        {
            TextAsset asset = new TextAsset(SampleClassContent);
            _toDestroy.Add(asset);

            bool result = ScriptUtility.TryGetDeclaredType(asset, out Type type);
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(SampleScriptableObject), type);
        }

        #endregion


        #region TryGetDeclaredTypeFromScriptAt

        [Test]
        public void TryGetDeclaredTypeFromScriptAt_NonExistentPath_ReturnsFalse()
        {
            string path = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.cs");
            bool result = ScriptUtility.TryGetDeclaredTypeFromScriptAt(path, out Type type);
            Assert.IsFalse(result);
            Assert.IsNull(type);
        }

        [Test]
        public void TryGetDeclaredTypeFromScriptAt_NonCsFile_ReturnsFalse()
        {
            // Exists, but the extension gate should reject it before parsing.
            string path = CreateTempFile(".txt", SampleClassContent);
            bool result = ScriptUtility.TryGetDeclaredTypeFromScriptAt(path, out Type type);
            Assert.IsFalse(result);
            Assert.IsNull(type);
        }

        [Test]
        public void TryGetDeclaredTypeFromScriptAt_ValidCsFile_ResolvesType()
        {
            string path = CreateTempFile(".cs", SampleClassContent);
            bool result = ScriptUtility.TryGetDeclaredTypeFromScriptAt(path, out Type type);
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(SampleScriptableObject), type);
        }

        #endregion


        #region ScriptRef

        [Test]
        public void ScriptRef_ScriptContent_ReadsFileContent()
        {
            string path = CreateTempFile(".cs", SampleClassContent);
            ScriptUtility.ScriptRef scriptRef = new ScriptUtility.ScriptRef(path);

            Assert.AreEqual(SampleClassContent, scriptRef.ScriptContent);
            StringAssert.Contains(Path.GetFileName(path), scriptRef.ScriptPath);
        }

        [Test]
        public void ScriptRef_ScriptContent_MissingFile_ReturnsNull()
        {
            string path = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.cs");
            ScriptUtility.ScriptRef scriptRef = new ScriptUtility.ScriptRef(path);

            Assert.IsNull(scriptRef.ScriptContent);
        }

        [Test]
        public void ScriptRef_GetScriptType_ValidScript_ResolvesType()
        {
            string path = CreateTempFile(".cs", SampleClassContent);
            ScriptUtility.ScriptRef scriptRef = new ScriptUtility.ScriptRef(path);

            bool result = scriptRef.GetScriptType(out Type type);
            Assert.IsTrue(result);
            Assert.AreEqual(typeof(SampleScriptableObject), type);
        }

        [Test]
        public void ScriptRef_GetScriptType_MissingFile_ReturnsFalseWithoutThrowing()
        {
            // Regression: a .cs path whose file doesn't exist used to feed a null to the regex and throw.
            string path = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.cs");
            ScriptUtility.ScriptRef scriptRef = new ScriptUtility.ScriptRef(path);

            Type type = null;
            bool result = false;
            Assert.DoesNotThrow(() => result = scriptRef.GetScriptType(out type));
            Assert.IsFalse(result);
            Assert.IsNull(type);
        }

        #endregion

    }

    /// <summary>
    /// Struct fixture used to verify struct resolution through <see cref="ScriptUtility.TryGetDeclaredType(string, out Type)"/>.
    /// </summary>
    internal struct ScriptUtilityStructSample { }

    /// <summary>
    /// Generic fixture used to verify generic-type (arity) resolution through
    /// <see cref="ScriptUtility.TryGetDeclaredType(string, out Type)"/>.
    /// </summary>
    internal class ScriptUtilityGenericSample<T> { }

}
