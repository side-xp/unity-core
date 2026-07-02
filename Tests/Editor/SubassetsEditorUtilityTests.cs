using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NUnit.Framework;

using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="SubassetsEditorUtility"/>.
    /// </summary>
    public class SubassetsEditorUtilityTests
    {

        #region GetAllowedSubassetsInfos

        [Test]
        public void GetAllowedSubassetsInfos_ReturnsConcreteSubtypesOnly()
        {
            Dictionary<Type, GUIContent> infos = SubassetsEditorUtility.GetAllowedSubassetsInfos(typeof(SubassetTestBase));

            Assert.IsTrue(infos.ContainsKey(typeof(SubassetTestAlpha)));
            Assert.IsTrue(infos.ContainsKey(typeof(SubassetTestBeta)));
            // Abstract and generic subtypes must be filtered out.
            Assert.IsFalse(infos.ContainsKey(typeof(SubassetTestAbstractChild)));
            Assert.IsFalse(infos.ContainsKey(typeof(SubassetTestGeneric<>)));
            Assert.AreEqual(2, infos.Count, "Only the two concrete, non-generic subtypes should be listed.");
        }

        [Test]
        public void GetAllowedSubassetsInfos_UsesLabelAttributeForNameAndTooltip()
        {
            Dictionary<Type, GUIContent> infos = SubassetsEditorUtility.GetAllowedSubassetsInfos(typeof(SubassetTestBase));

            GUIContent alpha = infos[typeof(SubassetTestAlpha)];
            Assert.AreEqual("Alpha Label", alpha.text);
            Assert.AreEqual("Alpha description", alpha.tooltip);
        }

        [Test]
        public void GetAllowedSubassetsInfos_FallsBackToNicifiedName()
        {
            Dictionary<Type, GUIContent> infos = SubassetsEditorUtility.GetAllowedSubassetsInfos(typeof(SubassetTestBase));

            GUIContent beta = infos[typeof(SubassetTestBeta)];
            Assert.AreEqual(ObjectNames.NicifyVariableName(nameof(SubassetTestBeta)), beta.text);
            Assert.IsTrue(string.IsNullOrEmpty(beta.tooltip), "A type without the label attribute should have no tooltip.");
        }

        [Test]
        public void GetAllowedSubassetsInfos_IsSortedByLabelText()
        {
            Dictionary<Type, GUIContent> infos = SubassetsEditorUtility.GetAllowedSubassetsInfos(typeof(SubassetTestBase));

            // The method inserts entries sorted by label text; Dictionary preserves insertion order while enumerating (no removals here).
            List<string> labels = new List<string>();
            foreach (GUIContent content in infos.Values)
                labels.Add(content.text);

            List<string> sorted = new List<string>(labels);
            sorted.Sort(StringComparer.Ordinal);
            CollectionAssert.AreEqual(sorted, labels, "Entries should be ordered by their display label.");
        }

        [Test]
        public void GetAllowedSubassetsInfos_NonScriptableObjectType_LogsErrorAndReturnsEmpty()
        {
            LogAssert.Expect(LogType.Error, new Regex("derive from " + nameof(ScriptableObject)));
            Dictionary<Type, GUIContent> infos = SubassetsEditorUtility.GetAllowedSubassetsInfos(typeof(int));
            Assert.IsEmpty(infos);
        }

        [Test]
        public void GetAllowedSubassetsInfos_NullType_LogsErrorAndReturnsEmpty()
        {
            LogAssert.Expect(LogType.Error, new Regex("derive from " + nameof(ScriptableObject)));
            Dictionary<Type, GUIContent> infos = SubassetsEditorUtility.GetAllowedSubassetsInfos(null);
            Assert.IsEmpty(infos);
        }

        #endregion

    }

    // Fixtures: a ScriptableObject subtype hierarchy exercised by TypeCache.GetTypesDerivedFrom.
    internal abstract class SubassetTestBase : ScriptableObject { }

    [SubassetLabel("Alpha Label", "Alpha description")]
    internal class SubassetTestAlpha : SubassetTestBase { }

    internal class SubassetTestBeta : SubassetTestBase { }

    internal abstract class SubassetTestAbstractChild : SubassetTestBase { }

    internal class SubassetTestGeneric<T> : SubassetTestBase { }

}
