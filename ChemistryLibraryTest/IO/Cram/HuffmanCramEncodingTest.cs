using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class HuffmanCramEncodingTest
    {
        [Test]
        public void IntegerRoundtrip()
        {
            var values = new[] { 86419, 619493, -10 };
            var codeComputer = new HuffmanCodeComputer();
            var sut = codeComputer.Compute(values);

            foreach (var value in values)
            {
                var encoded = sut.Encode(value);
                var decoded = sut.Decode(encoded);

                Assert.That(decoded, Is.EqualTo(value));
            }
        }

        [Test]
        public void ByteRoundtrip()
        {
            var values = new byte[] { 0x38, 0xb8, 0x03 };
            var codeComputer = new HuffmanCodeComputer();
            var sut = codeComputer.Compute(values);

            foreach (var value in values)
            {
                var encoded = sut.Encode(value);
                var decoded = sut.Decode(encoded);

                Assert.That(decoded, Is.EqualTo(value));
            }
        }
    }
}
