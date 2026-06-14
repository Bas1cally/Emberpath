using NUnit.Framework;

namespace Emberpath.Tests
{
    /// <summary>
    /// Minimal EditMode test. Its real job is to force CI to fully compile the
    /// project (gameplay scripts included) and to give the test runner a concrete
    /// test to execute, so a green run unambiguously means "everything compiled".
    /// </summary>
    public class SmokeTest
    {
        [Test]
        public void Project_Compiles_And_TestRunner_Works()
        {
            Assert.Pass("Emberpath scripts compiled and the EditMode test runner executed.");
        }
    }
}
