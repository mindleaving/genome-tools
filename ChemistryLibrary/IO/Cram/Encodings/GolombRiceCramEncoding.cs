using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class GolombRiceCramEncoding : ICramEncoding<int>
    {
        public GolombRiceCramEncoding(int offset, int log2OfM)
        {
            Offset = offset;
            Log2OfM = log2OfM;
        }

        public Codec CodecId => Codec.GolombRice;
        public int Offset { get; }
        public int Log2OfM { get; }

        public BitArray Encode(int item)
        {
            var offsetItem = item + Offset;
            var M = 1 << Log2OfM;
            var q = offsetItem / M;
            var r = offsetItem - q * M;

            var b = Log2OfM;
            var bits = new BitArray(q + 1 + b);
            EncodingHelpers.WriteUnary(bits, q, 0);
            WriteRemainder(bits, r, b);
            return bits;
        }

        private static void WriteRemainder(BitArray bits, int remainder, int b)
        {
            var remainderBits = new BitArray(BitConverter.GetBytes(remainder));
            for (int bitIndex = 0; bitIndex < b; bitIndex++)
            {
                var targetIndex = bits.Length - 1 - bitIndex;
                bits[targetIndex] = remainderBits[bitIndex];
            }
        }

        public int Decode(BitArray bits)
        {
            var q = EncodingHelpers.ReadUnary(bits);
            var b = Log2OfM;
            var r = 0;
            for (int bitIndex = 0; bitIndex < b-1; bitIndex++)
            {
                r = (r << 1) + (bits[bitIndex + q + 1] ? 1 : 0);
            }

            var x = bits[q+b] ? 1 : 0;
            r = r * 2 + x;
            return q * (1 << Log2OfM) + r - Offset;
        }
    }
}