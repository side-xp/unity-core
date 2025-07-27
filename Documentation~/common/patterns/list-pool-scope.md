# `ListPoolScope<T>` class

Gets or creates an instance of a list of a given type by using [`UnityEngine.ListPool<T>`](https://docs.unity3d.com/ScriptReference/Pool.ListPool_1.html), and makes sure that the list instance is released to the pool when disposed.

Because this class implements [`IDisposable`](https://learn.microsoft.com/dotnet/api/system.idisposable), a `ListPoolScope<T>` instance can be used in a *`using` block*, which guarantees that the `Dispose()` function is called at the end of that block.

Also note that `ListPoolScope<T>` implements all the functions of the [`List<T>`](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1) type, so you can use the scope just as you would for a list.

## Usage example

```cs
using UnityEngine;
using SideXP.Core;

public class ListPoolScopeComponent : MonoBehaviour
{
    private void Start()
    {
        GameObject[] filteredObjects = null;

        // A List instance will be get or created from the pool, and released at the end of this using block
        using (var list = new ListPoolScope<GameObject>())
        {
            // Add all the game objects from the scene
            list.AddRange(FindObjectsByType<GameObject>(FindObjectsSortMode.None));

            // Remove all UI objects (the ones with a RectComponent attached)
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].GetComponent<RectTransform>() != null)
                    list.RemoveAt(i);
            }

            // Sort the objects by name
            list.Sort((a, b) => a.name.CompareTo(b.name));
            filteredObjects = list.ToArray();
        }

        // Display the name of all the filtered objects in a console log
        Debug.Log("Filtered objects:\n" +
            string.Join("\n", filteredObjects.Map(i => i.name)));
    }
}
```