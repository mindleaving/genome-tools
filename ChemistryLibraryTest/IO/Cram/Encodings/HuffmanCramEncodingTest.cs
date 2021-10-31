using System.Collections.Generic;
using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Encodings
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
            var input = new BitStream(new byte[] { 0b11100000 });

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
            var streamBuffer = new byte[1];
            var stream = new BitStream(streamBuffer);

            sut.Encode('E', stream);
            stream.Flush();

            Assert.That(streamBuffer[0], Is.EqualTo(0b11100000));
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
            var input = new BitStream(new byte[] { 0b11100000 });

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
            var streamBuffer = new byte[1];
            var stream = new BitStream(streamBuffer);

            sut.Encode('E', stream);
            stream.Flush();

            Assert.That(streamBuffer[0], Is.EqualTo(0b11100000));
        }

        [Test]
        public void IntegerRoundtrip()
        {
            var values = new[] { 86419, 619493, -10 };
            var codeComputer = new HuffmanCodeComputer();
            var sut = codeComputer.Compute(values);

            foreach (var value in values)
            {
                var stream = new BitStream(new MemoryStream());
                sut.Encode(value, stream);
                stream.Seek(0, SeekOrigin.Begin);
                var decoded = sut.Decode(stream);

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
                var stream = new BitStream(new MemoryStream());
                sut.Encode(value, stream);
                stream.Seek(0, SeekOrigin.Begin);
                var decoded = sut.Decode(stream);

                Assert.That(decoded, Is.EqualTo(value));
            }
        }
    }
}
