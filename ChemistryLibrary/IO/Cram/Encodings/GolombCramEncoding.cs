using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class GolombCramEncoding : ICramEncoding<int>, ICramEncoding<byte>
    {
        public GolombCramEncoding(int offset, int m)
        {
            Offset = offset;
            M = m;
        }

        public Codec CodecId => Codec.Golomb;
        public int Offset { get; }
        public int M { get; }


        public void Encode(int item, BitStream stream)
        {
            var offsetItem = item + Offset;
            var q = offsetItem / M;
            var r = offsetItem - q * M;

            var b = CalculateB(M);
            stream.WriteUnary(q);
            WriteRemainder(stream, r, b, M);
        }

        public void Encode(byte item, BitStream stream)
        {
            Encode((int)item, stream);
        }

        private static int CalculateB(int M)
        {
            var log2M = EncodingHelpers.Log2Floor(M);
            return M == 1 << log2M ? log2M : log2M + 1;
        }

        private static void WriteRemainder(BitStream bits, int remainder, int b, int m)
        {
            int offsetRemainder;
            int bitsToWrite;
            if (remainder < (1 << b) - m)
            {
                offsetRemainder = remainder;
                bitsToWrite = b - 1;
            }
            else
            {
                offsetRemainder = remainder + (1 << b) - m;
                bitsToWrite = b;
            }
            var remainderBits = new BitArray(BitConverter.GetBytes(offsetRemainder));
            for (int bitIndex = 0; bitIndex < bitsToWrite; bitIndex++)
            {
                bits.WriteBit(remainderBits[bitsToWrite - 1 - bitIndex]);
            }
        }

        public int Decode(BitStream bits)
        {
            var q = bits.ReadUnary();
            var b = CalculateB(M);
            var r = 0;
            for (int bitIndex = 0; bitIndex < b-1; bitIndex++)
            {
                r = (r << 1) + (bits.ReadBit() ? 1 : 0);
            }

            if (r >= (1 << b) - M)
            {
                var x = bits.ReadBit() ? 1 : 0;
                r = r * 2 + x - ((1 << b) - M);
            }
            return q * M + r - Offset;
        }

        byte ICramEncoding<byte>.Decode(BitStream bits)
        {
            return (byte)Decode(bits);
        }
    }
}