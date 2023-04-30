using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Encodings
{
    public static class EncodingHelpers
    {
        public static int IndexOfLast1(BitArray bits)
        {
            var lastOneIndex = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    lastOneIndex = i;
            }
            return lastOneIndex;
        }

        public static void WriteUnary(this BitStream stream, int value)
        {
            for (int i = 0; i < value; i++)
            {
                stream.WriteBit(true);
            }
            stream.WriteBit(false);
        }

        public static int Log2Floor(int value)
        {
            var bits = new BitArray(BitConverter.GetBytes(value));
            return IndexOfLast1(bits);
        }

        public static int ReadUnary(this BitStream bits)
        {
            var u = 0;
            while (bits.ReadBit() == true)
            {
                u++;
            }
            return u;
        }

        public static int ReadInt32(this BitStream bits, int count)
        {
            var number = 0;
            for (int i = 0; i < count; i++)
            {
                var bit = bits.ReadBit();
                number = (number << 1) + (bit ? 1 : 0);
            }
            return number;
        }
    }
}
