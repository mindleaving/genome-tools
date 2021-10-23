using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class GammaCramEncodingTest
    {
        [Test]
        public void EncodingAsExpected()
        {
            var value = 10;
            var offset = 0;
            var sut = new GammaCramEncoding(offset);

            var actual = sut.Encode(value);

            Assert.That(actual.Length, Is.EqualTo(8)); // 10 needs 4 bits, prefix has same length => 2*4 = 8
            var bits = new bool[actual.Length];
            actual.CopyTo(bits, 0);
            Assert.That(bits, Is.EqualTo(new[] { false, false, false, false,  /**/  true, false, true, false }));
        }

        [Test]
        [TestCase(86419, 0)]
        [TestCase(86419, -10000)]
        [TestCase(-10, 20)]
        public void Roundtrip(int value, int offset)
        {
            var sut = new GammaCramEncoding(offset);
            var encoded = sut.Encode(value);
            var decoded = sut.Decode(encoded);

            Assert.That(decoded, Is.EqualTo(value));
        }

    }
}
