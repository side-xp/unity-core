using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class ListPoolScopeTests
    {

        [Test]
        public void Constructor_WithCapacity_IsReflectedByProperty()
        {
            // Regression guard: the Capacity property used to be a dead auto-property, so it returned 0
            // even after constructing with a capacity (the constructor sets the underlying list directly).
            using (var scope = new ListPoolScope<int>(16))
                Assert.AreEqual(16, scope.Capacity);
        }

        [Test]
        public void Capacity_SetterGetter_RoundTrips()
        {
            using (var scope = new ListPoolScope<int>())
            {
                scope.Capacity = 32;
                Assert.AreEqual(32, scope.Capacity);
            }
        }

        [Test]
        public void Capacity_BelowCount_Throws()
        {
            // List<T>.Capacity cannot be set below Count; this confirms Capacity is wired to the list
            // rather than to a disconnected backing field.
            using (var scope = new ListPoolScope<int>())
            {
                scope.Add(1);
                scope.Add(2);
                scope.Add(3);
                Assert.Throws<System.ArgumentOutOfRangeException>(() => scope.Capacity = 1);
            }
        }

        [Test]
        public void Capacity_IsAtLeastCount()
        {
            using (var scope = new ListPoolScope<int>())
            {
                scope.AddRange(new[] { 1, 2, 3, 4, 5 });
                Assert.GreaterOrEqual(scope.Capacity, scope.Count);
            }
        }

    }

}
