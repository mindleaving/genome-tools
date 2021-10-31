using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Encodings
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
            var stream = new BitStream(new MemoryStream());
            var sut = new GolombCramEncoding(offset, m);
            sut.Encode(value, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var decoded = sut.Decode(stream);

            Assert.That(decoded, Is.EqualTo(value));
        }
    }
}
