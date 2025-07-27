using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace SideXP.Core.EditorOnly
{

    /// <summary>
    /// Utility for drawing "drop zones" used to handle drag and drop for given types of objects.
    /// </summary>
    public static class DropZone
    {

        #region Delegates

        /// <summary>
        /// Called when objects are dropped in a drop zone using <see cref="Draw(Rect, Type[], DropDelegate, CanDropDelegate)"/>.
        /// </summary>
        /// <inheritdoc cref="DropDelegate{T}"/>
        public delegate bool DropDelegate(Object[] objectReferences);

        /// <inheritdoc cref="CanDropDelegate{T}"/>
        public delegate bool CanDropDelegate(Object obj);

        /// <summary>
        /// Called when objects are dropped in a drop zone using <see cref="Draw{T}(Rect, DropDelegate{T}, CanDropDelegate{T})"/>.
        /// </summary>
        /// <typeparam name="T">The type of objects allowed to be dropped in the drop zone.</typeparam>
        /// <param name="objectReferences">The filtered list of dragged objects.</param>
        /// <returns>Returns true if the objects have been dropped successfully.</returns>
        public delegate bool DropDelegate<T>(T[] objectReferences);

        /// <summary>
        /// Checks if an object can be dropped in a drop zone.<br/>
        /// Called when objects are dragged over the drop zone, as an additional check after ensuring the item has the expected type. So you don't need to check the type of the object by yourself at this point.
        /// </summary>
        /// <typeparam name="T">The type of objects allowed to be dropped in the drop zone.</typeparam>
        /// <param name="obj">The object being checked.</param>
        /// <returns>Returns true if the object can be dropped in the drop zone.</returns>
        public delegate bool CanDropDelegate<T>(T obj);

        #endregion


        #region Public API

        /// <summary>
        /// Draws a "drop zone" on the GUI, in which objects can be drag and dropped.
        /// </summary>
        /// <param name="position">The position and size of the drop zone on screen.</param>
        /// <param name="allowedTypes">The allowed object types for this drop zone.</param>
        /// <param name="onDrop">Called when items are dropped in this drop zone.</param>
        /// <param name="canDrop">Additional function to check if items can be dropped. Note that this is called after checking if the
        /// items have the appropriate types.</param>
        public static void Draw(Rect position, Type[] allowedTypes, DropDelegate onDrop, CanDropDelegate canDrop = null)
        {
            // Cancel if the mouse is not over the drop zone, or if no objects are dragged
            if (!position.Contains(Event.current.mousePosition) || DragAndDrop.objectReferences.Length <= 0)
                return;

            List<Object> draggedItems = new List<Object>();
            // For each dragged item
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                // For each allowed types
                foreach (Type t in allowedTypes)
                {
                    // If the dragged object has the expected type, add it to the list
                    if (obj.GetType() == t || obj.GetType().IsSubclassOf(t))
                    {
                        // Additional check if applicable
                        if (canDrop == null || canDrop(obj))
                            draggedItems.Add(obj);
                    }
                    // Special case for GameObjects: check if the target type is a component, and if so, check if the object is a GameObject that has that component
                    else if (t.IsSubclassOf(typeof(Component)) && obj.GetType() == typeof(GameObject))
                    {
                        if ((obj as GameObject).TryGetComponent(t, out Component comp))
                        {
                            // Additional check if applicable
                            if (canDrop == null || canDrop(obj))
                                draggedItems.Add(comp);
                        }
                    }
                }
            }

            // If there's at least one dragged object with the expected type
            if (draggedItems.Count > 0)
            {
                // If the drag operation is updated, change the cursor aspect
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                // if the drag operation is performed, resolve the operation
                else if (Event.current.type == EventType.DragPerform)
                {
                    if (onDrop != null && onDrop.Invoke(draggedItems.ToArray()))
                        Event.current.Use();
                }
            }
        }

        /// <inheritdoc cref="Draw{T1, T2, T3, T4}(Rect, DropDelegate, CanDropDelegate)"/>
        /// <typeparam name="T">The type of objects allowed to be dropped in this drop zone.</typeparam>
        public static void Draw<T>(Rect position, DropDelegate<T> onDrop, CanDropDelegate<T> canDrop = null)
           where T : Object
        {
            Draw
            (
                position,
                new Type[] { typeof(T) },
                // Convert onDrop function to non-generic delegate
                objs =>
                {
                    if (onDrop == null)
                        return false;

                    T[] castObjs = new T[objs.Length];
                    for (int i = 0; i < objs.Length; i++)
                        castObjs[i] = objs[i] as T;
                    return onDrop.Invoke(castObjs);
                },
                // Convert canDrop function to non-generic delegate
                obj => canDrop == null || canDrop.Invoke(obj as T)
            );
        }

        /// <inheritdoc cref="Draw{T1, T2, T3, T4}(Rect, DropDelegate, CanDropDelegate)"/>
        public static void Draw<T1, T2>(Rect position, DropDelegate onDrop, CanDropDelegate canDrop = null)
           where T1 : Object
           where T2 : Object
        {
            Draw(position, new Type[] { typeof(T1), typeof(T2) }, onDrop, canDrop);
        }

        /// <inheritdoc cref="Draw{T1, T2, T3, T4}(Rect, DropDelegate, CanDropDelegate)"/>
        public static void Draw<T1, T2, T3>(Rect position, DropDelegate onDrop, CanDropDelegate canDrop = null)
           where T1 : Object
           where T2 : Object
           where T3 : Object
        {
            Draw(position, new Type[] { typeof(T1), typeof(T2), typeof(T3) }, onDrop, canDrop);
        }

        /// <inheritdoc cref="Draw(Rect, Type[], DropDelegate, CanDropDelegate)"/>
        /// <typeparam name="T1">The first type of objects allowed to be dropped in this drop zone.</typeparam>
        /// <typeparam name="T2">The second type of objects allowed to be dropped in this drop zone.</typeparam>
        /// <typeparam name="T3">The third type of objects allowed to be dropped in this drop zone.</typeparam>
        /// <typeparam name="T4">The fourth type of objects allowed to be dropped in this drop zone.</typeparam>
        public static void Draw<T1, T2, T3, T4>(Rect position, DropDelegate onDrop, CanDropDelegate canDrop = null)
           where T1 : Object
           where T2 : Object
           where T3 : Object
           where T4 : Object
        {
            Draw(position, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, onDrop, canDrop);
        }

        #endregion

    }

}