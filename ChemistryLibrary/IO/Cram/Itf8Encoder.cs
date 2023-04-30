using System;
using System.Collections;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public static class Itf8Encoder
    {
        public static byte[] ToItf8(int number)
        {
            var bits = new BitArray(BitConverter.GetBytes(number));
            var bitsNeeded = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    bitsNeeded = i+1;
            }
            var bytesNeeded = bitsNeeded <= 7 ? 1 // The first byte including prefix (0b0) has room for 7 bits
                : bitsNeeded <= 14 ? 2 // The first byte now only has room for 6 bits after the prefix (0b10) + 8 bits for the second byte
                : bitsNeeded <= 21 ? 3
                : bitsNeeded <= 28 ? 4
                : 5;
            var buffer = new byte[bytesNeeded];
            for (int i = 0; i < bytesNeeded-1; i++)
            {
                buffer[0] = (byte)(buffer[0] | 1 << (7 - i));
            }

            var mask = bytesNeeded == 5 ? 0x0f : 0xff;
            var remainingNumber = number;
            for (int byteIndex = bytesNeeded-1; byteIndex >= 0; byteIndex--)
            {
                if (byteIndex == 0)
                {
                    buffer[byteIndex] = (byte)(buffer[byteIndex] | (remainingNumber & mask));
                }
                else
                {
                    buffer[byteIndex] = (byte)(remainingNumber & mask);
                }
                if (byteIndex == 4)
                {
                    mask = 0xff;
                    remainingNumber >>= 4;
                }
                else
                {
                    remainingNumber >>= 8;
                }
            }
            return buffer;
        }
    }
}
