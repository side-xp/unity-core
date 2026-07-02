using NUnit.Framework;

namespace SideXP.Core.Tests
{

    public class BoolExtensionsTests
    {

        [Test]
        public void ToLabel_DefaultLabels_ReturnTrueFalseStrings()
        {
            Assert.AreEqual(BoolExtensions.TrueLabel, true.ToLabel());
            Assert.AreEqual(BoolExtensions.FalseLabel, false.ToLabel());
        }

        [Test]
        public void ToLabel_DefaultLabels_HaveExpectedText()
        {
            Assert.AreEqual("true", true.ToLabel());
            Assert.AreEqual("false", false.ToLabel());
        }

        [Test]
        public void ToLabel_CustomLabels_ReturnMatchingBranch()
        {
            Assert.AreEqual("yes", true.ToLabel("yes", "no"));
            Assert.AreEqual("no", false.ToLabel("yes", "no"));
        }

        [Test]
        public void Constants_HaveExpectedValues()
        {
            Assert.AreEqual("true", BoolExtensions.TrueLabel);
            Assert.AreEqual("false", BoolExtensions.FalseLabel);
        }

    }

}
