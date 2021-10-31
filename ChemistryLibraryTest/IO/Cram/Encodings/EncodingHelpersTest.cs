using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Encodings
{
    public class EncodingHelpersTest
    {
        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(3, 1)]
        [TestCase(4, 2)]
        [TestCase(5, 2)]
        [TestCase(8, 3)]
        [TestCase(16, 4)]
        public void Log2FloorAsExpected(int input, int expected)
        {
            var actual = EncodingHelpers.Log2Floor(input);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
