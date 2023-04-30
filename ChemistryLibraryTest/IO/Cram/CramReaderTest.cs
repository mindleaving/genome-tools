using System.IO;
using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class CramReaderTest
    {
        [Test]
        public void SingleByteItf8IsRead()
        {
            var input = new[]{ (byte)0b00000011 };
            var reader = new CramBinaryReader(new MemoryStream(input));
            var actual = reader.ReadItf8();
            Assert.That(actual, Is.EqualTo(3));
        }

        [Test]
        public void TwoByteItf8IsRead()
        {
            var input = new[]{ (byte)0b10000011, (byte)0b10011011 };
            var reader = new CramBinaryReader(new MemoryStream(input));
            var actual = reader.ReadItf8();
            Assert.That(actual, Is.EqualTo(923));
        }

        [Test]
        public void FiveByteItf8IsRead()
        {
            var input = new[]{ (byte)0b11110011, (byte)0b10011011, (byte)0b10011101, (byte)0b01110110, (byte)0b00001010 };
            var reader = new CramBinaryReader(new MemoryStream(input));
            var actual = reader.ReadItf8();
            Assert.That(actual, Is.EqualTo(968480618));
        }

        [Test]
        public void FiveByteItf8IgnoresMostSignificantBitsOfFifthByte()
        {
            // Notice the most significant bits of 5th bytes compared to previous test.
            var input = new[]{ (byte)0b11110011, (byte)0b10011011, (byte)0b10011101, (byte)0b01110110, (byte)0b11111010 };
            var reader = new CramBinaryReader(new MemoryStream(input));
            var actual = reader.ReadItf8();
            Assert.That(actual, Is.EqualTo(968480618));
        }

        [Test]
        public void SingleByteLtf8IsRead()
        {
            var input = new[]{ (byte)0b00000011 };
            var reader = new CramBinaryReader(new MemoryStream(input));
            var actual = reader.ReadLtf8();
            Assert.That(actual, Is.EqualTo(3));
        }

        [Test]
        public void TwoByteLtf8IsRead()
        {
            var input = new[]{ (byte)0b10000011, (byte)0b10011011 };
            var reader = new CramBinaryReader(new MemoryStream(input));
            var actual = reader.ReadLtf8();
            Assert.That(actual, Is.EqualTo(923));
        }

        [Test]
        public void NinthByteLtf8IsRead()
        {
            var input = new[]{ (byte)0b11111111, (byte)0b01011011, (byte)0b10011101, (byte)0b01110110, (byte)0b00001010,
                (byte)0b10011011, (byte)0b10011101, (byte)0b01110110, (byte)0b11111010 };
            var reader = new CramBinaryReader(new MemoryStream(input));
            var actual = reader.ReadLtf8();
            Assert.That(actual, Is.EqualTo(6601562416727553786));
        }
    }
}
