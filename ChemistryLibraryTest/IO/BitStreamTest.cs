using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO
{
    public class BitStreamTest
    {
        [Test]
        public void InterlievedReadWriteAsExpected()
        {
            var bytes = new byte[] { 0xff, 0x01, 0x01 };
            var memoryStream = new MemoryStream(bytes);
            var sut = new ChemistryLibrary.IO.BitStream(memoryStream);

            // Byte 0
            var bit0 = sut.ReadBit(); 
            Assert.That(bit0, Is.True);

            sut.WriteBit(false);
            sut.WriteBit(false);
            sut.WriteBit(false);

            var bits4to7 = sut.ReadBits(4);
            Assert.That(bits4to7, Is.EqualTo(new[] { true, true, true, true}));

            // Byte 1
            var bit8 = sut.ReadBit();
            Assert.That(bit8, Is.False);

            sut.ReadBits(3);
            sut.WriteBit(true);
            sut.WriteBit(true);
            sut.WriteBit(true);
            sut.Seek(-1, SeekOrigin.Current);
            var bit8to15 = sut.ReadBits(8);
            Assert.That(bit8to15, Is.EqualTo(new[] { false, false, false, false, true, true, true, true }));

            // Byte 2
            sut.WriteBit(false);
            sut.WriteBit(false);
            sut.WriteBit(true);
            sut.WriteBit(true);
            sut.WriteBit(false);
            sut.WriteBit(false);
            sut.Seek(-1, SeekOrigin.Current);
            var bit16to23 = sut.ReadBits(8);
            Assert.That(bit16to23, Is.EqualTo(new[] { false, false, true, true, false, false, false, true }));
            
            // Non-existing byte 3
            Assert.That(() => sut.ReadBit(), Throws.Exception);
        }

        [Test]
        public void WritesAllBitsToStream()
        {
            var buffer = new byte[2];
            var sut = new BitStream(buffer);
            sut.WriteBit(true);
            sut.WriteBit(false);
            sut.WriteBit(true);
            sut.WriteBit(true);
            sut.WriteBit(false);
            sut.WriteBit(false);
            sut.WriteBit(false);
            sut.WriteBit(true);
            sut.WriteBit(true);
            sut.WriteBit(true);

            sut.Dispose();
            Assert.That(buffer, Is.EqualTo(new[] { 0b10110001, 0b11000000 }));

        }

        [Test]
        public void FlushOnlyWritesOnce()
        {
            var memoryStream = new MemoryStream();
            var sut = new BitStream(memoryStream);
            sut.WriteBit(false);
            sut.WriteBit(false);
            sut.WriteBit(false);
            sut.Flush();
            sut.WriteBit(false);
            sut.WriteBit(true);
            sut.WriteBit(false);
            sut.WriteBit(true);
            sut.WriteBit(false);
            sut.Flush();
            sut.Flush();

            var bytes = memoryStream.ToArray();
            Assert.That(bytes.Length, Is.EqualTo(1));
            Assert.That(bytes[0], Is.EqualTo(0b00001010));
        }
    }
}
