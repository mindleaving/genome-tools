using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramBinaryReader : IDisposable
    {
        private readonly BinaryReader reader;

        public CramBinaryReader(string filePath)
            : this(File.OpenRead(filePath))
        {
        }
        public CramBinaryReader(Stream stream, bool keepStreamOpen = false)
        {
            reader = new BinaryReader(stream, Encoding.UTF8, keepStreamOpen);
        }

        public void CheckFileFormat()
        {
            var fileDefinition = ReadFileDefinition(reader);
            if (fileDefinition.Version.Major != 3 && fileDefinition.Version.Minor != 0)
                throw new NotSupportedException("Only version 3.0 of the CRAM specification is currently supported");
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
        public void Seek(long offset, SeekOrigin origin) => reader.BaseStream.Seek(offset, origin);
        public byte ReadByte() => reader.ReadByte();
        public byte[] ReadBytes(int count) => reader.ReadBytes(count);
        public void Read(byte[] buffer, int offset, int count) => reader.Read(buffer, offset, count);
        public int ReadInt32() => reader.ReadInt32();
        public uint ReadUInt32() => reader.ReadUInt32();
        public char ReadChar() => reader.ReadChar();
        public sbyte ReadSByte() => reader.ReadSByte();
        public short ReadInt16() => reader.ReadInt16();
        public ushort ReadUInt16() => reader.ReadUInt16();
        public float ReadSingle() => reader.ReadSingle();
        public int ReadItf8() => reader.ReadItf8();
        public long ReadLtf8() => reader.ReadLtf8();

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

        public void ReadAndDiscardUnknownEncoding()
        {
            var codecId = (Codec)ReadItf8();
            var numberOfBytes = ReadItf8();
            ReadBytes(numberOfBytes);
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
                {
                    var offset = ReadItf8();
                    var m = ReadItf8();
                    return new GolombCramEncoding(offset, m);
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
    }
}
