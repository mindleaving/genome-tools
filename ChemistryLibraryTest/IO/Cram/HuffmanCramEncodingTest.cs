using System.Collections;
using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Cram;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class HuffmanCramEncodingTest
    {
        [Test]
        public void IntegerDecodeExample()
        {
            var symbolsWithCodeLength = new List<HuffmanCodeSymbol>
            {
                new('A', 1),
                new('B', 3),
                new('C', 3),
                new('D', 3),
                new('E', 4),
                new('F', 4)
            };
            var sut = new HuffmanIntCramEncoding(symbolsWithCodeLength);
            var input = new BitArray(new[] { true, true, true, false });

            var actual = sut.Decode(input);

            Assert.That(actual, Is.EqualTo('E'));
        }

        [Test]
        public void IntegerEncodeExample()
        {
            var symbolsWithCodeLength = new List<HuffmanCodeSymbol>
            {
                new('A', 1),
                new('B', 3),
                new('C', 3),
                new('D', 3),
                new('E', 4),
                new('F', 4)
            };
            var sut = new HuffmanIntCramEncoding(symbolsWithCodeLength);

            var actual = sut.Encode('E');

            Assert.That(actual, Is.EqualTo(new BitArray(new[] { true, true, true, false })));
        }

        [Test]
        public void ByteDecodeExample()
        {
            var symbolsWithCodeLength = new List<HuffmanCodeSymbol>
            {
                new('A', 1),
                new('B', 3),
                new('C', 3),
                new('D', 3),
                new('E', 4),
                new('F', 4)
            };
            var sut = new HuffmanByteCramEncoding(symbolsWithCodeLength);
            var input = new BitArray(new[] { true, true, true, false });

            var actual = sut.Decode(input);

            Assert.That(actual, Is.EqualTo('E'));
        }

        [Test]
        public void ByteEncodeExample()
        {
            var symbolsWithCodeLength = new List<HuffmanCodeSymbol>
            {
                new('A', 1),
                new('B', 3),
                new('C', 3),
                new('D', 3),
                new('E', 4),
                new('F', 4)
            };
            var sut = new HuffmanByteCramEncoding(symbolsWithCodeLength);

            var actual = sut.Encode('E');

            Assert.That(actual, Is.EqualTo(new BitArray(new[] { true, true, true, false })));
        }

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
