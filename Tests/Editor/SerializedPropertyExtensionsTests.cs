using System;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEditor;

using SideXP.Core.EditorOnly;

namespace SideXP.Core.Tests
{

    public class SerializedPropertyExtensionsTests
    {

        [Serializable]
        private class NestedData
        {
            public float value = 1.5f;
        }

        [Serializable]
        private class ContainerData
        {
            public int[] values = { 7, 8, 9 };
        }

        private class SampleSO : ScriptableObject
        {
            public int number = 5;
            public string text = "hello";
            public NestedData nested = new NestedData();
            public int[] numbers = { 1, 2, 3 };
            public List<NestedData> list = new List<NestedData> { new NestedData() };
            public List<ContainerData> containers = new List<ContainerData> { new ContainerData(), new ContainerData() };
            // A non-array field whose name merely contains "Array" — must NOT be treated as an array element.
            public int scoreArray = 0;
        }

        private SampleSO _target;
        private SerializedObject _serializedObject;

        [SetUp]
        public void SetUp()
        {
            _target = ScriptableObject.CreateInstance<SampleSO>();
            _serializedObject = new SerializedObject(_target);
        }

        [TearDown]
        public void TearDown()
        {
            _serializedObject?.Dispose();
            _serializedObject = null;
            if (_target != null)
                UnityEngine.Object.DestroyImmediate(_target);
            _target = null;
        }

        #region GetLabel

        [Test]
        public void GetLabel_UsesDisplayNameAndTooltip()
        {
            SerializedProperty property = _serializedObject.FindProperty(nameof(SampleSO.number));
            GUIContent label = property.GetLabel();
            Assert.AreEqual(property.displayName, label.text);
            Assert.AreEqual(property.tooltip, label.tooltip);
        }

        #endregion


        #region GetTarget

        [Test]
        public void GetTarget_SimpleField_ReturnsValue()
        {
            Assert.AreEqual(5, _serializedObject.FindProperty(nameof(SampleSO.number)).GetTarget());
            Assert.AreEqual("hello", _serializedObject.FindProperty(nameof(SampleSO.text)).GetTarget());
        }

        [Test]
        public void GetTarget_NestedField_ReturnsNestedInstance()
        {
            object target = _serializedObject.FindProperty(nameof(SampleSO.nested)).GetTarget();
            Assert.AreSame(_target.nested, target);
        }

        [Test]
        public void GetTarget_ArrayElement_ReturnsElementValue()
        {
            SerializedProperty element = _serializedObject.FindProperty(nameof(SampleSO.numbers)).GetArrayElementAtIndex(1);
            Assert.AreEqual(2, element.GetTarget());
        }

        [Test]
        public void GetTargetGeneric_CastsToRequestedType()
        {
            Assert.AreEqual(5, _serializedObject.FindProperty(nameof(SampleSO.number)).GetTarget<int>());

            SerializedProperty nestedValue = _serializedObject
                .FindProperty(nameof(SampleSO.nested))
                .FindPropertyRelative(nameof(NestedData.value));
            Assert.AreEqual(1.5f, nestedValue.GetTarget<float>());
        }

        #endregion


        #region GetTargetType / TryGetTargetType

        [Test]
        public void GetTargetType_SimpleFields_ReturnsDeclaredType()
        {
            Assert.AreEqual(typeof(int), _serializedObject.FindProperty(nameof(SampleSO.number)).GetTargetType());
            Assert.AreEqual(typeof(string), _serializedObject.FindProperty(nameof(SampleSO.text)).GetTargetType());
        }

        [Test]
        public void GetTargetType_NestedField_ReturnsNestedType()
        {
            Assert.AreEqual(typeof(NestedData), _serializedObject.FindProperty(nameof(SampleSO.nested)).GetTargetType());
        }

        [Test]
        public void GetTargetType_ArrayField_ReturnsArrayType()
        {
            Assert.AreEqual(typeof(int[]), _serializedObject.FindProperty(nameof(SampleSO.numbers)).GetTargetType());
        }

        [Test]
        public void TryGetTargetType_Resolvable_ReturnsTrueAndType()
        {
            bool success = _serializedObject
                .FindProperty(nameof(SampleSO.number))
                .TryGetTargetType(out Type type);
            Assert.IsTrue(success);
            Assert.AreEqual(typeof(int), type);
        }

        [Test]
        public void TryGetTargetType_UnresolvablePath_ReturnsFalseWithoutThrowing()
        {
            // Regression: TryGetTargetType used to dereference a null FieldOrPropertyInfo and throw.
            // GetFieldOrPropertyFromPath navigates arrays but not List<T>, so a list element's nested
            // field path resolves to null info — which must now yield false rather than an NRE.
            SerializedProperty listElementValue = _serializedObject
                .FindProperty(nameof(SampleSO.list))
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative(nameof(NestedData.value));
            Assert.IsNotNull(listElementValue, "the list element's 'value' property should exist");

            bool success = true;
            Type type = typeof(int);
            Assert.DoesNotThrow(() => success = listElementValue.TryGetTargetType(out type));
            Assert.IsFalse(success);
            Assert.IsNull(type);
        }

        #endregion


        #region IsArrayElement

        [Test]
        public void IsArrayElement_ArrayElement_ReturnsTrue()
        {
            SerializedProperty element = _serializedObject.FindProperty(nameof(SampleSO.numbers)).GetArrayElementAtIndex(0);
            Assert.IsTrue(element.IsArrayElement());
        }

        [Test]
        public void IsArrayElement_PlainField_ReturnsFalse()
        {
            Assert.IsFalse(_serializedObject.FindProperty(nameof(SampleSO.number)).IsArrayElement());
        }

        [Test]
        public void IsArrayElement_ArrayContainerItself_ReturnsFalse()
        {
            // The array field itself ("numbers") is not an element; only ".Array.data[i]" paths are.
            Assert.IsFalse(_serializedObject.FindProperty(nameof(SampleSO.numbers)).IsArrayElement());
        }

        [Test]
        public void IsArrayElement_FieldNamedLikeArray_ReturnsFalse()
        {
            // Regression: a bare Contains("Array") used to report true for any field whose name contains "Array".
            Assert.IsFalse(_serializedObject.FindProperty(nameof(SampleSO.scoreArray)).IsArrayElement());
        }

        #endregion


        #region TryGetArrayElementInfo

        [Test]
        public void TryGetArrayElementInfo_ArrayElement_ReturnsPathAndIndex()
        {
            SerializedProperty element = _serializedObject.FindProperty(nameof(SampleSO.numbers)).GetArrayElementAtIndex(2);
            Assert.IsTrue(element.TryGetArrayElementInfo(out string arrayPath, out int index));
            Assert.AreEqual("numbers", arrayPath);
            Assert.AreEqual(2, index);
        }

        [Test]
        public void TryGetArrayElementInfo_ListElement_ReturnsPathAndIndex()
        {
            SerializedProperty element = _serializedObject.FindProperty(nameof(SampleSO.list)).GetArrayElementAtIndex(0);
            Assert.IsTrue(element.TryGetArrayElementInfo(out string arrayPath, out int index));
            Assert.AreEqual("list", arrayPath);
            Assert.AreEqual(0, index);
        }

        [Test]
        public void TryGetArrayElementInfo_ElementOfNestedArray_ReturnsInnermostPathAndIndex()
        {
            SerializedProperty element = _serializedObject
                .FindProperty(nameof(SampleSO.containers))
                .GetArrayElementAtIndex(1)
                .FindPropertyRelative(nameof(ContainerData.values))
                .GetArrayElementAtIndex(2);
            Assert.IsTrue(element.TryGetArrayElementInfo(out string arrayPath, out int index));
            Assert.AreEqual("containers.Array.data[1].values", arrayPath);
            Assert.AreEqual(2, index);

            // The returned path must lead back to the containing array
            Assert.AreEqual(3, _serializedObject.FindProperty(arrayPath).arraySize);
        }

        [Test]
        public void TryGetArrayElementInfo_FieldNestedInElement_ReturnsFalse()
        {
            // Unlike IsArrayElement(), a field nested in an array element is not considered as an element itself
            SerializedProperty nestedField = _serializedObject
                .FindProperty(nameof(SampleSO.list))
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative(nameof(NestedData.value));
            Assert.IsFalse(nestedField.TryGetArrayElementInfo(out string arrayPath, out int index));
            Assert.IsNull(arrayPath);
            Assert.AreEqual(-1, index);
        }

        [Test]
        public void TryGetArrayElementInfo_NonElements_ReturnFalse()
        {
            Assert.IsFalse(_serializedObject.FindProperty(nameof(SampleSO.number)).TryGetArrayElementInfo(out _, out _));
            Assert.IsFalse(_serializedObject.FindProperty(nameof(SampleSO.numbers)).TryGetArrayElementInfo(out _, out _));
            Assert.IsFalse(_serializedObject.FindProperty(nameof(SampleSO.scoreArray)).TryGetArrayElementInfo(out _, out _));
        }

        #endregion

    }

}
