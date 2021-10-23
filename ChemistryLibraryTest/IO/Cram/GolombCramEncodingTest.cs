using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class GolombCramEncodingTest
    {
        [Test]
        [TestCase(86419, 0, 17)]
        [TestCase(86419, -10000, 17)]
        [TestCase(-10, 20, 10)]
        [TestCase(26, 0, 10)]
        public void Roundtrip(int value, int offset, int m)
        {
            var sut = new GolombCramEncoding(offset, m);
            var encoded = sut.Encode(value);
            var decoded = sut.Decode(encoded);

            Assert.That(decoded, Is.EqualTo(value));
        }
    }
}
