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

        public static void WriteUnary(BitArray bits, int value, int bitOffset)
        {
            for (int i = 0; i < value; i++)
            {
                bits[bitOffset + i] = true;
            }
            // Redundant: bits[bitOffset + value] = false;
        }

        public static int Log2Floor(int value)
        {
            var bits = new BitArray(BitConverter.GetBytes(value));
            return IndexOfLast1(bits);
        }

        public static int ReadUnary(BitArray bits)
        {
            var u = 0;
            while (bits[u] == true)
            {
                u++;
            }
            return u;
        }
    }
}
