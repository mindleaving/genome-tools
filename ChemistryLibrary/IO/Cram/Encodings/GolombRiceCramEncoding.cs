using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class GolombRiceCramEncoding : ICramEncoding<int>, ICramEncoding<byte>
    {
        public GolombRiceCramEncoding(int offset, int log2OfM)
        {
            Offset = offset;
            Log2OfM = log2OfM;
        }

        public Codec CodecId => Codec.GolombRice;
        public int Offset { get; }
        public int Log2OfM { get; }

        public void Encode(int item, BitStream stream)
        {
            var offsetItem = item + Offset;
            var M = 1 << Log2OfM;
            var q = offsetItem / M;
            var r = offsetItem - q * M;

            var b = Log2OfM;
            stream.WriteUnary(q);
            WriteRemainder(stream, r, b);
        }

        public void Encode(byte item, BitStream stream)
        {
            Encode((int)item, stream);
        }

        private static void WriteRemainder(BitStream bits, int remainder, int b)
        {
            var remainderBits = new BitArray(BitConverter.GetBytes(remainder));
            for (int bitIndex = 0; bitIndex < b; bitIndex++)
            {
                bits.WriteBit(remainderBits[b - 1 - bitIndex]);
            }
        }

        public int Decode(BitStream bits)
        {
            var q = bits.ReadUnary();
            var b = Log2OfM;
            var r = 0;
            for (int bitIndex = 0; bitIndex < b-1; bitIndex++)
            {
                r = (r << 1) + (bits.ReadBit() ? 1 : 0);
            }

            var x = bits.ReadBit() ? 1 : 0;
            r = r * 2 + x;
            return q * (1 << Log2OfM) + r - Offset;
        }

        byte ICramEncoding<byte>.Decode(BitStream bits)
        {
            return (byte)Decode(bits);
        }
    }
}