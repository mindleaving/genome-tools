using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramLoader
    {
        public CramLoaderResult Load(string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            using var reader = new BinaryReader(fileStream);

            var fileDefinition = ReadFileDefinition(reader);
            if (fileDefinition.Version.Major != 3 && fileDefinition.Version.Minor != 0)
                throw new NotSupportedException("Only version 3.0 of the CRAM specification is currently supported");

            var cramHeaderContainer = ReadCramHeaderContainer(reader);
            var dataContainers = ReadDataContainers(reader);
            var cramEofContainer = ReadCramEofContainer(reader);

            return new CramLoaderResult();
        }

        private CramFileDefinition ReadFileDefinition(BinaryReader reader)
        {
            var formatSpecifier = reader.ReadBytes(4);
            if (!formatSpecifier.SequenceEqual(Encoding.UTF8.GetBytes("CRAM")))
                throw new FormatException("File doesn't appear to be a CRAM file");
            var versionMajor = reader.ReadByte();
            var versionMinor = reader.ReadByte();
            var fileId = reader.ReadBytes(20);
            return new CramFileDefinition(new Version(versionMajor, versionMinor), fileId);
        }

        private object ReadCramHeaderContainer(BinaryReader reader)
        {
            // TODO: Move to other class
            var containerHeader = ReadContainerHeader(reader);

            var blocks = ReadBlocks(reader, containerHeader.NumberOfBlocks);

            SkipOverCramHeaderContainerNulPadding();
            throw new NotImplementedException();
        }

        private static CramContainerHeader ReadContainerHeader(BinaryReader reader)
        {
            var containerLength = reader.ReadInt32();
            var referenceSequenceId = reader.ReadItf8();
            var referenceStartingPosition = reader.ReadItf8();
            var alignmentSpan = reader.ReadItf8();
            var numberOfRecords = reader.ReadItf8();
            var recordCounter = reader.ReadLtf8();
            var numberOfReadBases = reader.ReadLtf8();
            var numberOfBlocks = reader.ReadItf8();
            var slicePositions = new List<int>();
            var checksum = reader.ReadInt32();
            return new CramContainerHeader(
                containerLength,
                referenceSequenceId,
                referenceStartingPosition,
                alignmentSpan,
                numberOfRecords,
                recordCounter,
                numberOfReadBases,
                numberOfBlocks,
                checksum,
                slicePositions);
        }

        private List<CramBlock> ReadBlocks(BinaryReader reader, int numberOfBlocks)
        {
            var blocks = new List<CramBlock>();
            for (int i = 0; i < numberOfBlocks; i++)
            {
                var block = ReadBlock(reader);
                blocks.Add(block);
            }
            return blocks;
        }

        private CramBlock ReadBlock(BinaryReader reader)
        {
            var blockHeader = ReadBlockHeader(reader);
            //var data = ;
            var checksum = reader.ReadInt32();

            return new CramBlock(blockHeader);
        }

        private PreservationMap ReadPreservationMap(BinaryReader reader)
        {
            var sizeInBytes = reader.ReadItf8();
            var numberOfEntries = reader.ReadItf8();
            var readNames = true;
            var apDataSeriesDelta = true;
            var referenceRequired = true;
            byte[] substitutionMatrix = null;
            var tagIds = new List<byte>();
            for (int i = 0; i < numberOfEntries; i++)
            {
                var key = Encoding.ASCII.GetString(reader.ReadBytes(2));
                switch (key)
                {
                    case "RN":
                        readNames = reader.ReadByte() == 0x1;
                        break;
                    case "AP":
                        apDataSeriesDelta = reader.ReadByte() == 0x1;
                        break;
                    case "RR":
                        referenceRequired = reader.ReadByte() == 0x1;
                        break;
                    case "SM":
                        substitutionMatrix = reader.ReadBytes(5);
                        break;
                    case "TD":
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(key), $"Unknown key '{key}' for preservation map");
                }
            }
            if (substitutionMatrix == null)
                throw new Exception("Preservation map is missing substitution matrix");
            // TODO: Check for valid tag IDs
            return new PreservationMap(
                readNames,
                apDataSeriesDelta,
                referenceRequired,
                substitutionMatrix,
                tagIds);
        }

        private void SkipOverCramHeaderContainerNulPadding()
        {
            throw new NotImplementedException();
        }

        private object ReadDataContainers(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        private object ReadCramEofContainer(BinaryReader reader)
        {
            var containerHeader = ReadContainerHeader(reader);
            var compressionHeader = ReadCompressionHeader(reader);
            throw new NotImplementedException();
        }

        private CramCompressionHeader ReadCompressionHeader(BinaryReader reader)
        {
            var blockHeader = ReadBlockHeader(reader);
            var preservationMap = ReadPreservationMap(reader);
            var dataSeriesEncoding = ReadDataSeriesEncoding(reader, preservationMap);
            var tagEncoding = ReadTagEncoding(reader, preservationMap);
            throw new NotImplementedException();
        }

        private object ReadTagEncoding(BinaryReader reader, PreservationMap preservationMap)
        {
            throw new NotImplementedException();
        }

        private DataSeriesEncoding ReadDataSeriesEncoding(BinaryReader reader, PreservationMap preservationMap)
        {
            var sizeInBytes = reader.ReadItf8();
            var numberOfEntries = reader.ReadItf8();

            for (int i = 0; i < numberOfEntries; i++)
            {
                var key = Encoding.ASCII.GetString(reader.ReadBytes(2));
                switch (key)
                {
                    case "BF":
                        break;
                    case "CF":
                        break;
                    case "RI":
                        break;
                    case "RL":
                        break;
                    case "AP":
                        break;
                    case "RG":
                        break;
                    case "RN":
                        break;
                    case "MF":
                        break;
                    case "NS":
                        break;
                    case "NP":
                        break;
                    case "TS":
                        break;
                    case "NF":
                        break;
                    case "TL":
                        break;
                    case "FN":
                        break;
                    case "FC":
                        break;
                    case "FP":
                        break;
                    case "DL":
                        break;
                    case "BB":
                        break;
                    case "QQ":
                        break;
                    case "BS":
                        break;
                    case "IN":
                        break;
                    case "RS":
                        break;
                    case "PD":
                        break;
                    case "HC":
                        break;
                    case "SC":
                        break;
                    case "MQ":
                        break;
                    case "BA":
                        break;
                    case "QS":
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(key), $"Unknown key '{key}' for data series encoding map");
                }
            }

            return new DataSeriesEncoding();
        }

        private CramBlockHeader ReadBlockHeader(BinaryReader reader)
        {
            var compressionMethod = (CramBlock.CompressionMethod)reader.ReadByte();
            var contentType = (CramBlock.BlockContentType)reader.ReadByte();
            var contentId = reader.ReadItf8();
            var compressedSize = reader.ReadItf8();
            var uncompressedSize = reader.ReadItf8();
            return new CramBlockHeader(
                compressionMethod,
                contentType,
                contentId,
                compressedSize,
                uncompressedSize);
        }
    }

    public class CramCompressionHeader
    {
    }

    public abstract class CramEncoding
    {
        public enum Codec
        {
            Null = 0,
            External = 1,
            Golomb = 2,
            Huffman = 3,
            ByteArrayLength = 4,
            ByteArrayStop = 5,
            Beta = 6,
            SubExponential = 7,
            GolombRice = 8,
            Gamma = 9
        }
        public abstract Codec CodecId { get; }
    }

    public class BetaCramEncoding : CramEncoding
    {
        public BetaCramEncoding(int offset, int numberOfBits)
        {
            Offset = offset;
            NumberOfBits = numberOfBits;
        }

        public override Codec CodecId => Codec.Beta;
        public int Offset { get; }
        public int NumberOfBits { get; }
    }

    public class SubExponentialCramEncoding : CramEncoding
    {
        public SubExponentialCramEncoding(int offset, int k)
        {
            Offset = offset;
            K = k;
        }

        public override Codec CodecId => Codec.SubExponential;
        public int Offset { get; }
        public int K { get; }
    }

    public class ByteArrayLengthCramEncoding : CramEncoding
    {
        public ByteArrayLengthCramEncoding(CramEncoding arrayLength, CramEncoding bytes)
        {
            ArrayLength = arrayLength;
            Bytes = bytes;
        }

        public override Codec CodecId => Codec.ByteArrayLength;
        public CramEncoding ArrayLength { get; }
        public CramEncoding Bytes { get; }
    }

    public class ByteArrayStopCramEncoding : CramEncoding
    {
        public ByteArrayStopCramEncoding(byte stopValue, int externalBlockContentId)
        {
            StopValue = stopValue;
            ExternalBlockContentId = externalBlockContentId;
        }

        public override Codec CodecId => Codec.ByteArrayStop;
        public byte StopValue { get; }
        public int ExternalBlockContentId { get; }
    }

    public class GammaCramEncoding : CramEncoding
    {
        public GammaCramEncoding(int offset)
        {
            Offset = offset;
        }

        public override Codec CodecId => Codec.Gamma;
        public int Offset { get; }
    }

    public class HuffmanCramEncoding : CramEncoding
    {
        public HuffmanCramEncoding(List<int> symbols, List<int> weights)
        {
            Symbols = symbols;
            Weights = weights;
        }

        public override Codec CodecId => Codec.Huffman;
        public List<int> Symbols { get; }
        public List<int> Weights { get; }
    }

    public class GolombCramEncoding : CramEncoding
    {
        public GolombCramEncoding(int offset, int m)
        {
            Offset = offset;
            M = m;
        }

        public override Codec CodecId => Codec.Golomb;
        public int Offset { get; }
        public int M { get; }
    }
    public class GolombRiceCramEncoding : CramEncoding
    {
        public GolombRiceCramEncoding(int offset, int log2OfM)
        {
            Offset = offset;
            Log2OfM = log2OfM;
        }

        public override Codec CodecId => Codec.GolombRice;
        public int Offset { get; }
        public int Log2OfM { get; }
    }
    public class ExternalCramEncoding : CramEncoding
    {
        public ExternalCramEncoding(int blockContentId)
        {
            BlockContentId = blockContentId;
        }

        public override Codec CodecId => Codec.External;
        public int BlockContentId { get; }
    }

    public class NullCramEncoding : CramEncoding
    {
        public override Codec CodecId => Codec.Null;
    }

    public class DataSeriesEncoding
    {
    }

    public class CramBlockHeader
    {
        public CramBlock.CompressionMethod CompressionMethod { get; }
        public CramBlock.BlockContentType ContentType { get; }
        public int ContentId { get; }
        public int CompressedSize { get; }
        public int UncompressedSize { get; }

        public CramBlockHeader(
            CramBlock.CompressionMethod compressionMethod, 
            CramBlock.BlockContentType contentType, 
            int contentId,
            int compressedSize, 
            int uncompressedSize)
        {
            CompressionMethod = compressionMethod;
            ContentType = contentType;
            ContentId = contentId;
            CompressedSize = compressedSize;
            UncompressedSize = uncompressedSize;
        }
    }

    public class CramContainerHeader
    {
        public int ContainerLength { get; }
        public int ReferenceSequenceId { get; }
        public int ReferenceStartingPosition { get; }
        public int AlignmentSpan { get; }
        public int NumberOfRecords { get; }
        public long RecordCounter { get; }
        public long NumberOfReadBases { get; }
        public int NumberOfBlocks { get; }
        public int Checksum { get; }
        public List<int> SlicePositions { get; }

        public CramContainerHeader(
            int containerLength, 
            int referenceSequenceId, 
            int referenceStartingPosition,
            int alignmentSpan, 
            int numberOfRecords, 
            long recordCounter,
            long numberOfReadBases, 
            int numberOfBlocks, 
            int checksum,
            List<int> slicePositions)
        {
            ContainerLength = containerLength;
            ReferenceSequenceId = referenceSequenceId;
            ReferenceStartingPosition = referenceStartingPosition;
            AlignmentSpan = alignmentSpan;
            NumberOfRecords = numberOfRecords;
            RecordCounter = recordCounter;
            NumberOfReadBases = numberOfReadBases;
            NumberOfBlocks = numberOfBlocks;
            Checksum = checksum;
            SlicePositions = slicePositions;
        }
    }

    public class PreservationMap
    {
        public bool ReadNames { get; }
        public bool ApDataSeriesDelta { get; }
        public bool ReferenceRequired { get; }
        public byte[] SubstitutionMatrix { get; }
        public List<byte> TagIds { get; }

        public PreservationMap(
            bool readNames, bool apDataSeriesDelta, bool referenceRequired,
            byte[] substitutionMatrix, List<byte> tagIds)
        {
            ReadNames = readNames;
            ApDataSeriesDelta = apDataSeriesDelta;
            ReferenceRequired = referenceRequired;
            SubstitutionMatrix = substitutionMatrix;
            TagIds = tagIds;
        }
    }

    public class CramBlock
    {
        public enum CompressionMethod
        {
            Raw = 0,
            Gzip = 1,
            Bzip2 = 2,
            Lzma = 3,
            Rans = 4
        }

        public enum BlockContentType
        {
            FileHeader = 0,
            CompressionHeader = 1,
            SliceHeader = 2,
            ExternalData = 4,
            CoreData = 5
        }

        public CramBlockHeader Header { get; }

        public CramBlock(CramBlockHeader header)
        {
            Header = header;
        }
    }

    public class CramFileDefinition
    {
        public Version Version { get; }
        public byte[] FileId { get; }

        public CramFileDefinition(Version version, byte[] fileId)
        {
            Version = version;
            FileId = fileId;
        }
    }

    public class CramLoaderResult 
    {

    }
}