using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class SubExponentialCramEncoding : ICramEncoding<int>
    {
        public SubExponentialCramEncoding(int offset, int k)
        {
            Offset = offset;
            K = k;
        }

        public Codec CodecId => Codec.SubExponential;
        public int Offset { get; }
        public int K { get; }

        public BitArray Encode(int item)
        {
            var offsetItem = item + Offset;
            var b = offsetItem < 1 << K 
                ? K 
                : EncodingHelpers.IndexOfLast1(new BitArray(BitConverter.GetBytes(offsetItem)));
            var u = offsetItem < 1 << K ? 0 : b - K + 1;
            var result = new BitArray(u + 1 + b);
            EncodingHelpers.WriteUnary(result, u, 0);
            WriteLessSignificantBits(result, offsetItem, b);

            return result;
        }

        private static void WriteLessSignificantBits(BitArray bits, int offsetItem, int numberOfBitsToWrite)
        {
            var itemBits = new BitArray(BitConverter.GetBytes(offsetItem));
            for (int bitIndex = 0; bitIndex < numberOfBitsToWrite; bitIndex++)
            {
                var sourceIndex = bitIndex;
                var targetIndex = bits.Length - 1 - bitIndex;
                bits[targetIndex] = itemBits[sourceIndex];
            }
        }

        public int Decode(BitArray bits)
        {
            var u = EncodingHelpers.ReadUnary(bits);
            int n;
            if (u == 0)
            {
                n = ToInt32(bits, 1);
            }
            else
            {
                var x = ToInt32(bits, u + 1);
                n = (1 << (u + K - 1)) + x;
            }
            return n - Offset;
        }

        private int ToInt32(BitArray bits, int offset)
        {
            var number = 0;
            for (int bitIndex = offset; bitIndex < bits.Length; bitIndex++)
            {
                number = (number << 1) + (bits[bitIndex] ? 1 : 0);
            }
            return number;
        }
    }
}