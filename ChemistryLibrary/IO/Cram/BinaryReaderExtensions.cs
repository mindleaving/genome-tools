using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public static class BinaryReaderExtensions
    {
        const int BitsPerByte = 8;
        public static int ReadItf8(this BinaryReader reader)
        {
            var prefix = reader.ReadByte();
            var bytesToRead = GetBytesToRead(prefix, 4);
            if (bytesToRead > 4)
                throw new OverflowException("Tried to read ITF8 but number of bytes to read was greater than 4");
            var buffer = new byte[4]; // Make static for performance?
            reader.Read(buffer, 0, bytesToRead);
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

        public static long ReadLtf8(this BinaryReader reader)
        {
            var prefix = reader.ReadByte();
            var bytesToRead = GetBytesToRead(prefix, 8);
            var buffer = new byte[8]; // Make static for performance?
            reader.Read(buffer, 0, bytesToRead);
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

        public static ICramEncoding<int> ReadIntegerEncoding(this BinaryReader reader)
        {
            var codecId = (Codec)reader.ReadItf8();
            var numberOfBytes = reader.ReadItf8();
            switch (codecId)
            {
                case Codec.Null:
                {
                    return new NullCramEncoding<int>();
                }
                case Codec.External:
                {
                    var blockContentId = reader.ReadItf8();
                    return new ExternalCramEncoding<int>(blockContentId);
                }
                case Codec.Golomb:
                {
                    var offset = reader.ReadItf8();
                    var m = reader.ReadItf8();
                    return new GolombCramEncoding(offset, m);
                }
                case Codec.Huffman:
                {
                    var symbols = reader.ReadCramItf8Array();
                    var weights = reader.ReadCramItf8Array();
                    return new HuffmanIntCramEncoding(symbols, weights);
                }
                case Codec.Beta:
                {
                    var offset = reader.ReadItf8();
                    var numberOfBits = reader.ReadItf8();
                    return new BetaCramEncoding(offset, numberOfBits);
                }
                case Codec.SubExponential:
                {
                    var offset = reader.ReadItf8();
                    var k = reader.ReadItf8();
                    return new SubExponentialCramEncoding(offset, k);
                }
                case Codec.GolombRice:
                {
                    var offset = reader.ReadItf8();
                    var log2OfM = reader.ReadItf8();
                    return new GolombRiceCramEncoding(offset, log2OfM);
                }
                case Codec.Gamma:
                {
                    var offset = reader.ReadItf8();
                    return new GammaCramEncoding(offset);
                }
                case Codec.ByteArrayLength:
                case Codec.ByteArrayStop:
                    throw new Exception($"Requested an integer encoding but codec '{codecId}' doesn't support integers");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ICramEncoding<byte> ReadByteEncoding(this BinaryReader reader)
        {
            var codecId = (Codec)reader.ReadItf8();
            var numberOfBytes = reader.ReadItf8();
            switch (codecId)
            {
                case Codec.Null:
                {
                    return new NullCramEncoding<byte>();
                }
                case Codec.External:
                {
                    var blockContentId = reader.ReadItf8();
                    return new ExternalCramEncoding<byte>(blockContentId);
                }
                case Codec.Huffman:
                {
                    var symbols = reader.ReadCramItf8Array();
                    var weights = reader.ReadCramItf8Array();
                    return new HuffmanByteCramEncoding(symbols.Cast<byte>().ToList(), weights);
                }
                case Codec.Golomb:
                case Codec.ByteArrayLength:
                case Codec.ByteArrayStop:
                case Codec.Beta:
                case Codec.SubExponential:
                case Codec.GolombRice:
                case Codec.Gamma:
                    throw new Exception($"Requested a byte encoding but codec '{codecId}' doesn't support bytes");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static ICramEncoding<byte[]> ReadByteArrayEncoding(this BinaryReader reader)
        {
            var codecId = (Codec)reader.ReadItf8();
            var numberOfBytes = reader.ReadItf8();
            switch (codecId)
            {
                case Codec.Null:
                {
                    return new NullCramEncoding<byte[]>();
                }
                case Codec.External:
                {
                    var blockContentId = reader.ReadItf8();
                    return new ExternalCramEncoding<byte[]>(blockContentId);
                }
                case Codec.ByteArrayLength:
                {
                    var arrayLength = reader.ReadIntegerEncoding();
                    var bytes = reader.ReadByteEncoding();
                    return new ByteArrayLengthCramEncoding(arrayLength, bytes);
                }
                case Codec.ByteArrayStop:
                {
                    var stopValue = reader.ReadByte();
                    var externalBlockContentId = reader.ReadItf8();
                    return new ByteArrayStopCramEncoding(stopValue, externalBlockContentId);
                }
                case Codec.Golomb:
                case Codec.Huffman:
                case Codec.Beta:
                case Codec.SubExponential:
                case Codec.GolombRice:
                case Codec.Gamma:
                    throw new Exception($"Requested a byte array encoding but codec '{codecId}' doesn't support byte arrays");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static List<int> ReadCramItf8Array(this BinaryReader reader)
        {
            var arrayLength = reader.ReadItf8();
            var values = new List<int>();
            for (int i = 0; i < arrayLength; i++)
            {
                var value = reader.ReadItf8();
                values.Add(value);
            }
            return values;
        }

        public static byte[] ReadCramByteArray(this BinaryReader reader)
        {
            var arrayLength = reader.ReadItf8();
            var values = new List<byte>();
            for (int i = 0; i < arrayLength; i++)
            {
                var value = reader.ReadByte();
                values.Add(value);
            }
            return values.ToArray();
        }
        
    }
}