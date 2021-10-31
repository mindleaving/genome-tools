using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Encodings
{
    public class SubExponentialCramEncodingTest
    {
        [Test]
        [TestCase(86419, 0, 17)]
        [TestCase(86419, -10000, 17)]
        [TestCase(-10, 20, 17)]
        [TestCase(86419, 0, 3)]
        [TestCase(86419, -10000, 3)]
        [TestCase(-10, 20, 1)]
        public void Roundtrip(int value, int offset, int k)
        {
            var sut = new SubExponentialCramEncoding(offset, k);
            var stream = new BitStream(new MemoryStream());

            sut.Encode(value, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var decoded = sut.Decode(stream);

            Assert.That(decoded, Is.EqualTo(value));
        }
    }
}
