using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class GolombCramEncoding : ICramEncoding<int>
    {
        public GolombCramEncoding(int offset, int m)
        {
            Offset = offset;
            M = m;
        }

        public Codec CodecId => Codec.Golomb;
        public int Offset { get; }
        public int M { get; }


        public BitArray Encode(int item)
        {
            var offsetItem = item + Offset;
            var q = offsetItem / M;
            var r = offsetItem - q * M;

            var b = CalculateB(M);
            var remainderBitCount = r < (1 << b) - M ? b - 1 : b;
            var bits = new BitArray(q + 1 + remainderBitCount);
            EncodingHelpers.WriteUnary(bits, q, 0);
            WriteRemainder(bits, r, b, M);
            return bits;
        }

        private static int CalculateB(int M)
        {
            var log2M = EncodingHelpers.Log2Floor(M);
            return M == 1 << log2M ? log2M : log2M + 1;
        }

        private static void WriteRemainder(BitArray bits, int remainder, int b, int m)
        {
            int offsetRemainder;
            if (remainder < (1 << b) - m)
            {
                offsetRemainder = remainder;
            }
            else
            {
                offsetRemainder = remainder + (1 << b) - m;
            }
            var remainderBits = new BitArray(BitConverter.GetBytes(offsetRemainder));
            for (int bitIndex = 0; bitIndex < b; bitIndex++)
            {
                var targetIndex = bits.Length - 1 - bitIndex;
                bits[targetIndex] = remainderBits[bitIndex];
            }
        }

        public int Decode(BitArray bits)
        {
            var q = EncodingHelpers.ReadUnary(bits);
            var b = CalculateB(M);
            var r = 0;
            for (int bitIndex = 0; bitIndex < b-1; bitIndex++)
            {
                r = (r << 1) + (bits[bitIndex + q + 1] ? 1 : 0);
            }

            if (r >= (1 << b) - M)
            {
                var x = bits[q+b] ? 1 : 0;
                r = r * 2 + x - ((1 << b) - M);
            }
            return q * M + r - Offset;
        }
    }
}