using GenomeTools.ChemistryLibrary.IO.Cram;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class GolombRiceCramEncodingTest
    {
        [Test]
        [TestCase(86419, 0, 4)]
        [TestCase(86419, -10000, 4)]
        [TestCase(-10, 20, 3)]
        [TestCase(26, 0, 3)]
        public void Roundtrip(int value, int offset, int m)
        {
            var sut = new GolombRiceCramEncoding(offset, m);
            var encoded = sut.Encode(value);
            var decoded = sut.Decode(encoded);

            Assert.That(decoded, Is.EqualTo(value));
        }
    }
}
