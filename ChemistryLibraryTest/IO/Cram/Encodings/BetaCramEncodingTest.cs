using System;
using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Encodings
{
    public class BetaCramEncodingTest
    {
        [Test]
        [TestCase(86419, 0, 17)]
        [TestCase(86419, -10000, 17)]
        [TestCase(-10, 20, 17)]
        public void Roundtrip(int value, int offset, int numberOfBits)
        {
            var sut = new BetaCramEncoding(offset, numberOfBits);
            var stream = new BitStream(new MemoryStream());

            sut.Encode(value, stream);
            stream.Seek(0, SeekOrigin.Begin);
            var decoded = sut.Decode(stream);

            Assert.That(decoded, Is.EqualTo(value));
        }

        [Test]
        public void NonEncodableNumbersThrowException()
        {
            var value = 18;
            var offset = 0;
            var numberOfBits = 4; // Max number: 15
            var stream = new BitStream(new MemoryStream());
            var sut = new BetaCramEncoding(offset, numberOfBits); 

            Assert.That(() => sut.Encode(value, stream), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
