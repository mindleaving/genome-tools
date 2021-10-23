using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramReader : IDisposable
    {
        const int BitsPerByte = 8;

        private readonly BinaryReader reader;

        public CramReader(string filePath)
            : this(File.OpenRead(filePath))
        {
        }
        public CramReader(Stream stream, bool keepStreamOpen = false)
        {
            reader = CreateReader(stream, keepStreamOpen);
        }

        private static BinaryReader CreateReader(Stream stream, bool keepStreamOpen)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, keepStreamOpen);

            var fileDefinition = ReadFileDefinition(reader);
            if (fileDefinition.Version.Major != 3 && fileDefinition.Version.Minor != 0)
                throw new NotSupportedException("Only version 3.0 of the CRAM specification is currently supported");
            return reader;
        }

        private static CramFileDefinition ReadFileDefinition(BinaryReader reader)
        {
            var formatSpecifier = reader.ReadBytes(4);
            if (!formatSpecifier.SequenceEqual(Encoding.UTF8.GetBytes("CRAM")))
                throw new FormatException("File doesn't appear to be a CRAM file");
            var versionMajor = reader.ReadByte();
            var versionMinor = reader.ReadByte();
            var fileId = reader.ReadBytes(20);
            return new CramFileDefinition(new Version(versionMajor, versionMinor), fileId);
        }

        public long Position => reader.BaseStream.Position;

        public void Seek(long offset, SeekOrigin origin)
        {
            reader.BaseStream.Seek(offset, origin);
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            reader.Read(buffer, offset, count);
        }

        public int ReadInt32()
        {
            return reader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return reader.ReadUInt32();
        }

        public char ReadChar()
        {
            return reader.ReadChar();
        }

        public int ReadItf8()
        {
            var prefix = ReadByte();
            var bytesToRead = GetBytesToRead(prefix, 4);
            if (bytesToRead > 4)
                throw new OverflowException("Tried to read ITF8 but number of bytes to read was greater than 4");
            var buffer = new byte[4]; // Make static for performance?
            Read(buffer, 0, bytesToRead);
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

        public long ReadLtf8()
        {
            var prefix = ReadByte();
            var bytesToRead = GetBytesToRead(prefix, 8);
            var buffer = new byte[8]; // Make static for performance?
            Read(buffer, 0, bytesToRead);
            var bitsFromPrefix = ((prefix << bytesToRead) & 0xff) >> bytesToRead; // Zero out leading 1's
            long decodedNumber = bitsFromPrefix;
            for (int i = 0; i < bytesToRead; i++)
            {
                decodedNumber = (decodedNumber << BitsPerByte) + buffer[i];
            }
            return decodedNumber;
        }

        private int GetBytesToRead(byte prefix, int maxBytes)
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

        public ICramEncoding<int> ReadIntegerEncoding()
        {
            var codecId = (Codec)ReadItf8();
            var numberOfBytes = ReadItf8();
            switch (codecId)
            {
                case Codec.Null:
                {
                    return new NullCramEncoding<int>();
                }
                case Codec.External:
                {
                    var blockContentId = ReadItf8();
                    return new ExternalCramEncoding<int>(blockContentId);
                }
                case Codec.Golomb:
                {
                    var offset = ReadItf8();
                    var m = ReadItf8();
                    return new GolombCramEncoding(offset, m);
                }
                case Codec.Huffman:
                {
                    var symbols = ReadCramItf8Array();
                    var codeLengths = ReadCramItf8Array();
                    var huffmanCodeSymbols = symbols
                        .Zip(codeLengths, (symbol, codeLength) => new HuffmanCodeSymbol(symbol, codeLength))
                        .ToList();
                    return new HuffmanIntCramEncoding(huffmanCodeSymbols);
                }
                case Codec.Beta:
                {
                    var offset = ReadItf8();
                    var numberOfBits = ReadItf8();
                    return new BetaCramEncoding(offset, numberOfBits);
                }
                case Codec.SubExponential:
                {
                    var offset = ReadItf8();
                    var k = ReadItf8();
                    return new SubExponentialCramEncoding(offset, k);
                }
                case Codec.GolombRice:
                {
                    var offset = ReadItf8();
                    var log2OfM = ReadItf8();
                    return new GolombRiceCramEncoding(offset, log2OfM);
                }
                case Codec.Gamma:
                {
                    var offset = ReadItf8();
                    return new GammaCramEncoding(offset);
                }
                case Codec.ByteArrayLength:
                case Codec.ByteArrayStop:
                    throw new Exception($"Requested an integer encoding but codec '{codecId}' doesn't support integers");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ICramEncoding<byte> ReadByteEncoding()
        {
            var codecId = (Codec)ReadItf8();
            var numberOfBytes = ReadItf8();
            switch (codecId)
            {
                case Codec.Null:
                {
                    return new NullCramEncoding<byte>();
                }
                case Codec.External:
                {
                    var blockContentId = ReadItf8();
                    return new ExternalCramEncoding<byte>(blockContentId);
                }
                case Codec.Huffman:
                {
                    var symbols = ReadCramItf8Array();
                    var codeLengths = ReadCramItf8Array();
                    var huffmanCodeSymbols = symbols
                        .Zip(codeLengths, (symbol, codeLength) => new HuffmanCodeSymbol(symbol, codeLength))
                        .ToList();
                    return new HuffmanByteCramEncoding(huffmanCodeSymbols);
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

        public ICramEncoding<byte[]> ReadByteArrayEncoding()
        {
            var codecId = (Codec)ReadItf8();
            var numberOfBytes = ReadItf8();
            switch (codecId)
            {
                case Codec.Null:
                {
                    return new NullCramEncoding<byte[]>();
                }
                case Codec.External:
                {
                    var blockContentId = ReadItf8();
                    return new ExternalCramEncoding<byte[]>(blockContentId);
                }
                case Codec.ByteArrayLength:
                {
                    var arrayLength = ReadIntegerEncoding();
                    var bytes = ReadByteEncoding();
                    return new ByteArrayLengthCramEncoding(arrayLength, bytes);
                }
                case Codec.ByteArrayStop:
                {
                    var stopValue = ReadByte();
                    var externalBlockContentId = ReadItf8();
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

        public List<int> ReadCramItf8Array()
        {
            var arrayLength = ReadItf8();
            var values = new List<int>();
            for (int i = 0; i < arrayLength; i++)
            {
                var value = ReadItf8();
                values.Add(value);
            }
            return values;
        }

        public byte[] ReadCramByteArray()
        {
            var arrayLength = ReadItf8();
            var values = new List<byte>();
            for (int i = 0; i < arrayLength; i++)
            {
                var value = ReadByte();
                values.Add(value);
            }
            return values.ToArray();
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public sbyte ReadSByte()
        {
            return reader.ReadSByte();
        }

        public short ReadInt16()
        {
            return reader.ReadInt16();
        }

        public ushort ReadUInt16()
        {
            return reader.ReadUInt16();
        }

        public float ReadSingle()
        {
            return reader.ReadSingle();
        }
    }
}
