using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class BetaCramEncoding : ICramEncoding<int>
    {
        public BetaCramEncoding(int offset, int numberOfBits)
        {
            Offset = offset;
            NumberOfBits = numberOfBits;
        }

        public Codec CodecId => Codec.Beta;
        public int Offset { get; }
        public int NumberOfBits { get; }


        public BitArray Encode(int item)
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
            var bigEndianBits = new BitArray(NumberOfBits);
            CopyLittleEndianToBitEndian(littleEndianBits, bigEndianBits, NumberOfBits);
            return bigEndianBits;
        }

        private void CopyLittleEndianToBitEndian(BitArray source, BitArray target, int numberOfBits)
        {
            for (int bitIndex = 0; bitIndex < numberOfBits; bitIndex++)
            {
                var sourceIndex = bitIndex;
                var targetIndex = target.Length - 1 - bitIndex;
                target[targetIndex] = source[sourceIndex];
            }
        }

        public int Decode(BitArray bits)
        {
            var number = 0;
            for (int bitIndex = 0; bitIndex < NumberOfBits; bitIndex++)
            {
                number = (number << 1) + (bits[bitIndex] ? 1 : 0);
            }
            return number - Offset;
        }
    }
}