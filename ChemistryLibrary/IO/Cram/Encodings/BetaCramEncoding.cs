using System;
using System.Collections;
using System.IO;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class BetaCramEncoding : ICramEncoding<int>, ICramEncoding<byte>
    {
        public BetaCramEncoding(int offset, int numberOfBits)
        {
            Offset = offset;
            NumberOfBits = numberOfBits;
        }

        public Codec CodecId => Codec.Beta;
        public int Offset { get; }
        public int NumberOfBits { get; }


        public void Encode(byte item, BitStream stream)
        {
            Encode((int)item, stream);
        }

        public void Encode(int item, BitStream stream)
        {
            var offsetItem = item + Offset;
            if (offsetItem >= 1 << NumberOfBits)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(item),
                    $"Value to be encoded is greater than the capacity of this codec. Value (after applying offset): {offsetItem}. Max value: {1 << NumberOfBits}");
            }
            var bytes = BitConverter.GetBytes(offsetItem);
            var littleEndianBits = new BitArray(bytes);
            CopyLittleEndianToBitEndian(littleEndianBits, stream, NumberOfBits);
        }

        private void CopyLittleEndianToBitEndian(BitArray source, BitStream target, int numberOfBits)
        {
            for (int bitIndex = 0; bitIndex < numberOfBits; bitIndex++)
            {
                var sourceIndex = numberOfBits - 1 - bitIndex;
                target.WriteBit(source[sourceIndex]);
            }
        }

        byte ICramEncoding<byte>.Decode(BitStream bits)
        {
            return (byte)Decode(bits);
        }

        public int Decode(BitStream bits)
        {
            var number = 0;
            for (int bitIndex = 0; bitIndex < NumberOfBits; bitIndex++)
            {
                var bit = bits.ReadBit();
                number = (number << 1) + (bit ? 1 : 0);
            }
            return number - Offset;
        }
    }
}