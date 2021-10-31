using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public class GammaCramEncoding : ICramEncoding<int>, ICramEncoding<byte>
    {
        public GammaCramEncoding(int offset)
        {
            Offset = offset;
        }

        public Codec CodecId => Codec.Gamma;
        public int Offset { get; }


        public void Encode(int item, BitStream stream)
        {
            var offsetItem = item + Offset;
            var bits = new BitArray(BitConverter.GetBytes(offsetItem));
            var n = EncodingHelpers.IndexOfLast1(bits)+1;
            for (int i = 0; i < n; i++)
            {
                stream.WriteBit(false);
            }
            CopyBitsLittleToBigEndian(bits, stream, n);
        }

        public void Encode(byte item, BitStream stream)
        {
            Encode((int)item, stream);
        }

        private void CopyBitsLittleToBigEndian(BitArray source, BitStream target, int bitCount)
        {
            for (int bitIndex = 0; bitIndex < bitCount; bitIndex++)
            {
                var sourceIndex = bitCount - 1 - bitIndex;
                target.WriteBit(source[sourceIndex]);
            }
        }

        public int Decode(BitStream bits)
        {
            var n = CountZeros(bits);
            var result = (1 << (n-1)) + bits.ReadInt32(n-1) - Offset;
            return result;
        }

        byte ICramEncoding<byte>.Decode(BitStream bits)
        {
            return (byte)Decode(bits);
        }

        private int CountZeros(BitStream bits)
        {
            var count = 0;
            while (bits.ReadBit() == false)
            {
                count++;
            }
            return count;
        }
    }
}