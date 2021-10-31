using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Encodings
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
            var stream = new BitStream(new MemoryStream());

            sut.Encode(value, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var decoded = sut.Decode(stream);

            Assert.That(decoded, Is.EqualTo(value));
        }
    }
}
