using System;
using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
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
            var encoded = sut.Encode(value);
            var decoded = sut.Decode(encoded);

            Assert.That(decoded, Is.EqualTo(value));
        }

        [Test]
        public void NonEncodableNumbersThrowException()
        {
            var value = 18;
            var offset = 0;
            var numberOfBits = 4; // Max number: 15
            var sut = new BetaCramEncoding(offset, numberOfBits); 

            Assert.That(() => sut.Encode(value), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
