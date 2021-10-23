using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class GammaCramEncoding : ICramEncoding<int>
    {
        public GammaCramEncoding(int offset)
        {
            Offset = offset;
        }

        public Codec CodecId => Codec.Gamma;
        public int Offset { get; }


        public BitArray Encode(int item)
        {
            var offsetItem = item + Offset;
            var bits = new BitArray(BitConverter.GetBytes(offsetItem));
            var n = EncodingHelpers.IndexOfLast1(bits)+1;
            var result = new BitArray(2 * n);
            CopyBitsLittleToBigEndian(bits, result, n);
            return result;
        }

        private void CopyBitsLittleToBigEndian(BitArray source, BitArray target, int bitCount)
        {
            for (int bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                var targetIndex = target.Length - 1 - bitIndex;
                var sourceIndex = bitIndex;
                target[targetIndex] = source[sourceIndex];
            }
        }

        public int Decode(BitArray bits)
        {
            var n = CountZeros(bits);
            var result = ToInt32(bits, n) - Offset;
            return result;
        }

        private int ToInt32(BitArray bits, int startIndex)
        {
            var number = 0;
            for (int i = startIndex; i < bits.Length; i++)
            {
                number = (number << 1) + (bits[i] ? 1 : 0);
            }
            return number;
        }

        private int CountZeros(BitArray bits)
        {
            var count = 0;
            while (bits[count] == false)
            {
                count++;
            }
            return count;
        }
    }
}