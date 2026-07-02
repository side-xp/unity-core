using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class SingletonTests
    {

        // A concrete singleton used only by these tests. Its static instance persists for the
        // lifetime of the test run, which is exactly the singleton behavior being verified.
        private class SampleSingleton : Singleton<SampleSingleton>
        {
            public static int InitCount = 0;
            public int Payload = 0;

            protected override void Init()
            {
                InitCount++;
                Payload = 42;
            }
        }

        [Test]
        public void Instance_FirstAccess_IsNotNull()
        {
            Assert.IsNotNull(SampleSingleton.Instance);
        }

        [Test]
        public void Instance_MultipleAccesses_ReturnSameObject()
        {
            SampleSingleton first = SampleSingleton.Instance;
            SampleSingleton second = SampleSingleton.Instance;
            Assert.AreSame(first, second);
        }

        [Test]
        public void I_IsAliasForInstance()
        {
            Assert.AreSame(SampleSingleton.Instance, SampleSingleton.I);
        }

        [Test]
        public void Init_RunsOnCreation_AppliesSideEffects()
        {
            // Init() sets Payload; observing 42 confirms the hook ran when the instance was created.
            Assert.AreEqual(42, SampleSingleton.Instance.Payload);
        }

        [Test]
        public void Init_AcrossManyAccesses_CalledExactlyOnce()
        {
            // The instance is created at most once ever, so InitCount stabilizes at 1 regardless
            // of how many times Instance is read here or in other tests.
            _ = SampleSingleton.Instance;
            _ = SampleSingleton.Instance;
            _ = SampleSingleton.Instance;
            Assert.AreEqual(1, SampleSingleton.InitCount);
        }

    }

}
