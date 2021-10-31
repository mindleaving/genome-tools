using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Encodings
{
    public class GammaCramEncodingTest
    {
        [Test]
        public void EncodingAsExpected()
        {
            var value = 10;
            var offset = 0;
            var sut = new GammaCramEncoding(offset);
            var streamBuffer = new byte[2];
            var stream = new BitStream(streamBuffer);

            sut.Encode(value, stream);
            stream.Flush();

            Assert.That(streamBuffer[0], Is.EqualTo(0b00001010));
            Assert.That(streamBuffer[1], Is.EqualTo(0x00));
        }

        [Test]
        [TestCase(86419, 0)]
        [TestCase(86419, -10000)]
        [TestCase(-10, 20)]
        public void Roundtrip(int value, int offset)
        {
            var memoryStream = new MemoryStream();
            var stream = new BitStream(memoryStream);
            var sut = new GammaCramEncoding(offset);
            sut.Encode(value, stream);
            var bytes = memoryStream.ToArray();
            stream.Seek(0, SeekOrigin.Begin);
            var decoded = sut.Decode(stream);

            Assert.That(decoded, Is.EqualTo(value));
        }

    }
}
