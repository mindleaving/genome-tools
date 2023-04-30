using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class SubExponentialCramEncoding : ICramEncoding<int>, ICramEncoding<byte>
    {
        public SubExponentialCramEncoding(int offset, int k)
        {
            Offset = offset;
            K = k;
        }

        public Codec CodecId => Codec.SubExponential;
        public int Offset { get; }
        public int K { get; }

        public void Encode(int item, BitStream stream)
        {
            var offsetItem = item + Offset;
            var b = offsetItem < 1 << K 
                ? K 
                : EncodingHelpers.IndexOfLast1(new BitArray(BitConverter.GetBytes(offsetItem)));
            var u = offsetItem < 1 << K ? 0 : b - K + 1;
            stream.WriteUnary(u);
            WriteLessSignificantBits(stream, offsetItem, b);
        }

        public void Encode(byte item, BitStream stream)
        {
            Encode((int)item, stream);
        }

        private static void WriteLessSignificantBits(BitStream bits, int offsetItem, int numberOfBitsToWrite)
        {
            var itemBits = new BitArray(BitConverter.GetBytes(offsetItem));
            for (int bitIndex = 0; bitIndex < numberOfBitsToWrite; bitIndex++)
            {
                var sourceIndex = numberOfBitsToWrite - 1 - bitIndex;
                bits.WriteBit(itemBits[sourceIndex]);
            }
        }

        public int Decode(BitStream bits)
        {
            var u = bits.ReadUnary();
            int n;
            if (u == 0)
            {
                n = bits.ReadInt32(K);
            }
            else
            {
                var x = bits.ReadInt32(u + K - 1);
                n = (1 << (u + K - 1)) + x;
            }
            return n - Offset;
        }

        byte ICramEncoding<byte>.Decode(BitStream bits)
        {
            return (byte)Decode(bits);
        }
    }
}