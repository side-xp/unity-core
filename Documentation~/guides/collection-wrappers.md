# Collection wrappers

When you create a custom property drawer to change the GUI of a single property, it applies only to that property. That’s usually the intended behavior, but if you have an array of elements that are decorated with a custom property drawer, that GUI is apply to each property and not to the list itself.

This type is meant to “wrap” a list (`ListWrapper` class) or an array (`ArrayWrapper` class), so you can create a custom property drawer for the entire list instead of creating one for each item.

The built-in implementations include a property `_elements` that stores the items of the collection, and which is the one meant to be decorated by a custom property drawer.

## Usage example

In a component:

```cs
[SerializeField]
[MetadataCollectionAttribute] // Used by the custom property drawer to identify this type of property
private ArrayWrapper<ScriptableObject> _assets = new ArrayWrapper<ScriptableObject>();
```

Custom attribute definition:

```cs
using UnityEngine;
public class MetadataCollectionAttribute : PropertyAttribute { }
```

Custom property drawer:

```cs
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MetadataCollectionAttribute), false)]
public class MetadataCollectionPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty elementsProp = property.FindPropertyRelative(CollectionWrapperUtility.ElementsProp);
        EditorGUI.PropertyField(position, elementsProp, label);
    }
}
```