using System;

using NUnit.Framework;

using UnityEngine;
using UnityEditor;

using SideXP.Core.EditorOnly;

using Object = UnityEngine.Object;

namespace SideXP.Core.Tests
{

    public class EditorHelpersTests
    {

        #region Samples

        private class LabelSample
        {
            [Tooltip("The number tooltip")]
            public int number;

            public string noTooltip;

            [Tooltip("The property tooltip")]
            public float SomeProperty { get; set; }
        }

        // Throwaway EditorWindow subclasses. Created via CreateInstance (loaded, findable by Resources.FindObjectsOfTypeAll)
        // and destroyed after each test — no window is ever shown.
        private class TestWindowBase : EditorWindow { }
        private class TestWindowDerived : TestWindowBase { }

        private static TestWindowBase CreateWindow(string title = null)
        {
            var window = ScriptableObject.CreateInstance<TestWindowBase>();
            if (title != null)
                window.titleContent = new GUIContent(title);
            return window;
        }

        #endregion


        #region GetTooltip

        [Test]
        public void GetTooltip_FieldWithTooltip_ReturnsTooltip()
        {
            Assert.AreEqual("The number tooltip", EditorHelpers.GetTooltip(typeof(LabelSample), "number"));
        }

        [Test]
        public void GetTooltip_PropertyWithTooltip_ReturnsTooltip()
        {
            Assert.AreEqual("The property tooltip", EditorHelpers.GetTooltip(typeof(LabelSample), "SomeProperty"));
        }

        [Test]
        public void GetTooltip_FieldWithoutTooltip_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, EditorHelpers.GetTooltip(typeof(LabelSample), "noTooltip"));
        }

        [Test]
        public void GetTooltip_UnknownMember_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, EditorHelpers.GetTooltip(typeof(LabelSample), "doesNotExist"));
        }

        [Test]
        public void GetTooltip_GenericAndObjectOverloads_Delegate()
        {
            Assert.AreEqual("The number tooltip", EditorHelpers.GetTooltip<LabelSample>("number"));
            Assert.AreEqual("The number tooltip", EditorHelpers.GetTooltip(new LabelSample(), "number"));
        }

        #endregion


        #region GetLabel

        [Test]
        public void GetLabel_NicifiesNameAndPullsTooltip()
        {
            GUIContent label = EditorHelpers.GetLabel(typeof(LabelSample), "number");
            Assert.AreEqual(ObjectNames.NicifyVariableName("number"), label.text);
            Assert.AreEqual("The number tooltip", label.tooltip);
        }

        [Test]
        public void GetLabel_CustomLabel_OverridesText()
        {
            GUIContent label = EditorHelpers.GetLabel(typeof(LabelSample), "number", "Custom Label");
            Assert.AreEqual("Custom Label", label.text);
            Assert.AreEqual("The number tooltip", label.tooltip, "the tooltip still comes from the member");
        }

        [Test]
        public void GetLabel_NoTooltip_HasEmptyTooltip()
        {
            GUIContent label = EditorHelpers.GetLabel(typeof(LabelSample), "noTooltip");
            Assert.AreEqual(ObjectNames.NicifyVariableName("noTooltip"), label.text);
            Assert.AreEqual(string.Empty, label.tooltip);
        }

        [Test]
        public void GetLabel_GenericAndObjectOverloads_Delegate()
        {
            Assert.AreEqual(ObjectNames.NicifyVariableName("number"), EditorHelpers.GetLabel<LabelSample>("number").text);
            Assert.AreEqual(ObjectNames.NicifyVariableName("number"), EditorHelpers.GetLabel(new LabelSample(), "number").text);
        }

        #endregion


        #region FindEditorWindow

        [Test]
        public void FindEditorWindow_ByExactType_ReturnsInstance()
        {
            TestWindowBase window = CreateWindow();
            try
            {
                Assert.AreSame(window, EditorHelpers.FindEditorWindow(typeof(TestWindowBase)));
            }
            finally
            {
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void FindEditorWindow_Generic_ReturnsInstance()
        {
            TestWindowBase window = CreateWindow();
            try
            {
                Assert.AreSame(window, EditorHelpers.FindEditorWindow<TestWindowBase>());
            }
            finally
            {
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void FindEditorWindow_ByTitle_ReturnsInstance()
        {
            string title = "SideXP_TestWindow_" + Guid.NewGuid().ToString("N");
            TestWindowBase window = CreateWindow(title);
            try
            {
                Assert.AreSame(window, EditorHelpers.FindEditorWindow(title));
            }
            finally
            {
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void FindEditorWindow_ByBaseType_ReturnsInstance()
        {
            // Regression: the inverted IsAssignableFrom returned null for base-type queries; a TestWindowBase
            // instance IS an EditorWindow, so the query must resolve to a non-null window.
            TestWindowBase window = CreateWindow();
            try
            {
                Assert.IsNotNull(EditorHelpers.FindEditorWindow(typeof(EditorWindow)));
            }
            finally
            {
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void FindEditorWindow_DerivedTypeWithNoInstance_ReturnsNull()
        {
            // Regression: the inverted check returned the base-type window for a derived query. Only a
            // TestWindowBase exists here (never a TestWindowDerived), so the derived query must be null.
            TestWindowBase window = CreateWindow();
            try
            {
                Assert.IsNull(EditorHelpers.FindEditorWindow(typeof(TestWindowDerived)));
            }
            finally
            {
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void FindEditorWindow_OutOverload_Found_ReturnsTrue()
        {
            TestWindowBase window = CreateWindow();
            try
            {
                Assert.IsTrue(EditorHelpers.FindEditorWindow(typeof(TestWindowBase), out EditorWindow found));
                Assert.AreSame(window, found);
            }
            finally
            {
                Object.DestroyImmediate(window);
            }
        }

        [Test]
        public void FindEditorWindow_OutOverload_NotFound_ReturnsFalse()
        {
            bool success = EditorHelpers.FindEditorWindow<TestWindowDerived>(out TestWindowDerived found);
            Assert.IsFalse(success);
            Assert.IsNull(found);
        }

        #endregion

    }

}
