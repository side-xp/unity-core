using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

namespace SideXP.Core.Tests
{

    /// <summary>
    /// Tests for <see cref="RendererExtensions"/>.
    /// </summary>
    /// <remarks>Uses a mesh renderer (a primitive cube), whose <see cref="Renderer.bounds"/> is valid at edit time — unlike
    /// collider bounds, which are only reliable in play mode.</remarks>
    public class RendererExtensionsTests
    {

        private const float Tolerance = 1e-3f;

        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _objects)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _objects.Clear();
        }

        private GameObject NewCube(Vector3 position, Vector3 scale)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); // unit-sized mesh + MeshRenderer
            go.transform.position = position;
            go.transform.localScale = scale;
            _objects.Add(go);
            return go;
        }

        private static void AssertApproximately(Vector3 expected, Vector3 actual, string message = null)
        {
            Assert.Less(Vector3.Distance(expected, actual), Tolerance, message ?? $"Expected {expected}, got {actual}.");
        }

        [Test]
        public void GetRendererBounds_OutOverload_ReturnsTrueWithWorldBounds()
        {
            GameObject cube = NewCube(new Vector3(3f, -1f, 2f), Vector3.one);
            Renderer renderer = cube.GetComponent<Renderer>();

            Assert.IsTrue(renderer.GetRendererBounds(out Bounds bounds));
            AssertApproximately(cube.transform.position, bounds.center, "Bounds should be centered on the cube.");
            AssertApproximately(Vector3.one, bounds.size, "A unit cube should have unit-sized bounds.");
        }

        [Test]
        public void GetRendererBounds_NonOutOverload_MatchesOutOverload()
        {
            Renderer renderer = NewCube(new Vector3(1f, 1f, 1f), Vector3.one).GetComponent<Renderer>();

            renderer.GetRendererBounds(out Bounds expected);
            Bounds bounds = renderer.GetRendererBounds();

            AssertApproximately(expected.center, bounds.center);
            AssertApproximately(expected.size, bounds.size);
        }

        [Test]
        public void GetRendererBounds_ScaledObject_ReflectsScale()
        {
            Renderer renderer = NewCube(Vector3.zero, new Vector3(2f, 3f, 4f)).GetComponent<Renderer>();

            renderer.GetRendererBounds(out Bounds bounds);

            // A unit cube scaled by (2,3,4) yields bounds of that size.
            AssertApproximately(new Vector3(2f, 3f, 4f), bounds.size);
        }

    }

}
