using System;
using System.IO;

namespace GenomeTools.ChemistryLibrary.Extensions
{
    public static class StreamExtensions
    {
        public const int BitsPerByte = 8;

        public static int ReadItf8(this BinaryReader stream)
        {
            var prefix = stream.ReadByte();
            var bytesToRead = GetBytesToRead(prefix, 4);
            if (bytesToRead > 4)
                throw new OverflowException("Tried to read ITF8 but number of bytes to read was greater than 4");
            var buffer = new byte[4]; // Make static for performance?
            stream.Read(buffer, 0, bytesToRead);
            var bitsFromPrefix = ((prefix << bytesToRead) & 0xff) >> bytesToRead; // Zero out leading 1's
            var decodedNumber = bitsFromPrefix;
            for (int i = 0; i < bytesToRead; i++)
            {
                if (i == buffer.Length - 1)
                    decodedNumber = (decodedNumber << 4) + (buffer[i] & 0x0f); // Only use last 4 bits of 5th byte
                else
                    decodedNumber = (decodedNumber << BitsPerByte) + buffer[i];
            }
            return decodedNumber;
        }

        public static long ReadLtf8(this BinaryReader stream)
        {
            var prefix = stream.ReadByte();
            var bytesToRead = GetBytesToRead(prefix, 8);
            var buffer = new byte[8]; // Make static for performance?
            stream.Read(buffer, 0, bytesToRead);
            var bitsFromPrefix = ((prefix << bytesToRead) & 0xff) >> bytesToRead; // Zero out leading 1's
            long decodedNumber = bitsFromPrefix;
            for (int i = 0; i < bytesToRead; i++)
            {
                decodedNumber = (decodedNumber << BitsPerByte) + buffer[i];
            }
            return decodedNumber;
        }

        private static int GetBytesToRead(byte prefix, int maxBytes)
        {
            var bytesToRead = 0;
            var bitPosition = BitsPerByte - 1;
            while (bitPosition >= 0 && (prefix & (byte)(0x1 << bitPosition)) > 0)
            {
                bytesToRead++;
                if (bytesToRead == maxBytes)
                    return maxBytes;
                bitPosition--;
            }
            return bytesToRead;
        }
    }
}
