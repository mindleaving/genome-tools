using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Sam;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramLoader
    {
        private readonly SamHeaderParser samHeaderParser = new();

        public CramLoaderResult Load(string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            using var reader = new BinaryReader(fileStream);

            var fileDefinition = ReadFileDefinition(reader);
            if (fileDefinition.Version.Major != 3 && fileDefinition.Version.Minor != 0)
                throw new NotSupportedException("Only version 3.0 of the CRAM specification is currently supported");

            var cramHeader = ReadCramHeaderContainer(reader);
            var dataContainers = ReadDataContainers(reader);
            if (!IsEofContainer(dataContainers.Last()))
                throw new FormatException("No EOF-container found. File is truncated");

            return new CramLoaderResult(cramHeader, dataContainers);
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

        private CramHeader ReadCramHeaderContainer(BinaryReader reader)
        {
            // TODO: Move to other class
            var containerHeader = ReadContainerHeader(reader);

            var blocks = ReadBlocks(reader, containerHeader.NumberOfBlocks, null);
            var samHeader = Encoding.ASCII.GetString(blocks[0].UncompressedDecodedData);
            samHeader = samHeader.Substring(4); // TODO: Why are there 4 bytes before the header entries? String length?
            var samHeaderLines = samHeader.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var samHeaderEntries = samHeaderLines
                .Where(line => line.StartsWith("@"))
                .Select(samHeaderParser.Parse)
                .ToList();
            return new CramHeader(samHeaderEntries);
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
            var slicePositions = reader.ReadCramItf8Array();
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

        private List<CramBlock> ReadBlocks(
            BinaryReader reader, 
            int numberOfBlocks,
            CramCompressionHeader compressionHeader)
        {
            var blocks = new List<CramBlock>();
            for (int i = 0; i < numberOfBlocks; i++)
            {
                var block = ReadBlock(reader, compressionHeader);
                blocks.Add(block);
            }
            return blocks;
        }

        private CramBlock ReadBlock(BinaryReader reader, CramCompressionHeader compressionHeader)
        {
            var blockHeader = ReadBlockHeader(reader);
            var compressedData = reader.ReadBytes(blockHeader.CompressedSize);
            var uncompressedData = UncompressBlockData(compressedData, blockHeader);
            var decodedData = DecodeBlockData(uncompressedData, compressionHeader);
            var checksum = reader.ReadInt32();

            return new CramBlock(blockHeader, decodedData, checksum);
        }

        private byte[] DecodeBlockData(byte[] uncompressedData, CramCompressionHeader compressionHeader)
        {
            // TODO
            return uncompressedData;
        }

        private byte[] UncompressBlockData(byte[] compressedData, CramBlockHeader blockHeader)
        {
            switch (blockHeader.CompressionMethod)
            {
                case CramBlock.CompressionMethod.Raw:
                    return compressedData;
                case CramBlock.CompressionMethod.Gzip:
                {
                    using var instream = new MemoryStream(compressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    GZip.Decompress(instream, outstream, false);
                    return outstream.ToArray();
                }
                case CramBlock.CompressionMethod.Bzip2:
                {
                    using var instream = new MemoryStream(compressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    BZip2.Decompress(instream, outstream, false);
                    return outstream.ToArray();
                }
                case CramBlock.CompressionMethod.Lzma:
                    break;
                case CramBlock.CompressionMethod.Rans:
                {
                    using var instream = new MemoryStream(compressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    RansDecoder.Decode(instream, outstream);
                    var uncompressBlockData = outstream.ToArray();
                    return uncompressBlockData;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

        private PreservationMap ReadPreservationMap(BinaryReader reader, CramBlockHeader blockHeader)
        {
            var sizeInBytes = reader.ReadItf8();
            var numberOfEntries = reader.ReadItf8();
            var readNames = true;
            var apDataSeriesDelta = true;
            var referenceRequired = true;
            byte[] substitutionMatrix = Array.Empty<byte>();
            var tagIdCombinations = new List<List<TagId>>();
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
                        var tagIdBytes = reader.ReadCramByteArray();
                        tagIdCombinations.AddRange(ParseTagIdBytes(tagIdBytes));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(key), $"Unknown key '{key}' for preservation map");
                }
            }
            //if (substitutionMatrix == null)
            //    throw new Exception("Preservation map is missing substitution matrix");
            // TODO: Check for valid tag IDs
            return new PreservationMap(
                readNames,
                apDataSeriesDelta,
                referenceRequired,
                substitutionMatrix,
                tagIdCombinations);
        }

        private List<List<TagId>> ParseTagIdBytes(byte[] tagIdBytes)
        {
            return tagIdBytes.Split<byte>(0x00, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseTagCombination)
                .ToList();
        }

        private List<TagId> ParseTagCombination(IEnumerable<byte> combinationBytes)
        {
            var tagIds = new List<TagId>();
            var tagIdBytes = new byte[3];
            var tagIdByteIndex = 0;
            foreach (var combinationByte in combinationBytes)
            {
                tagIdBytes[tagIdByteIndex] = combinationByte;
                tagIdByteIndex++;
                if (tagIdByteIndex == tagIdBytes.Length)
                {
                    var tag = Encoding.ASCII.GetString(tagIdBytes, 0, 2);
                    var valueType = (char)tagIdBytes[2];
                    var tagId = new TagId(tag, valueType);
                    tagIds.Add(tagId);
                    tagIdByteIndex = 0;
                }
            }

            return tagIds;
        }

        private List<CramDataContainer> ReadDataContainers(BinaryReader reader)
        {
            var dataContainers = new List<CramDataContainer>();
            while (true)
            {
                var dataContainer = ReadDataContainer(reader);
                if (IsEofContainer(dataContainer))
                {
                    dataContainers.Add(new CramEofContainer());
                    return dataContainers;
                }
                dataContainers.Add(dataContainer);
            }
        }

        private bool IsEofContainer(CramDataContainer dataContainer)
        {
            return dataContainer.ContainerHeader.NumberOfBlocks == 1
                && dataContainer.ContainerHeader.Checksum == 1339669765
                && dataContainer.CompressionHeader.Checksum == 1258382318;
        }

        private CramDataContainer ReadDataContainer(BinaryReader reader)
        {
            var containerHeader = ReadContainerHeader(reader);
            var sliceOffset = reader.BaseStream.Position;
            var compressionHeader = ReadCompressionHeader(reader);
            var slices = ReadSlices(reader, containerHeader, sliceOffset, compressionHeader);
            return new CramDataContainer(containerHeader, compressionHeader, slices);
        }

        private List<CramSlice> ReadSlices(BinaryReader reader, CramContainerHeader containerHeader, long sliceOffset, CramCompressionHeader compressionHeader)
        {
            var slices = new List<CramSlice>();
            foreach (var slicePosition in containerHeader.SlicePositions)
            {
                var slice = ReadSlice(reader, slicePosition, sliceOffset, compressionHeader);
                slices.Add(slice);
            }
            return slices;
        }

        private CramSlice ReadSlice(
            BinaryReader reader, int slicePosition, long firstSliceOffset,
            CramCompressionHeader compressionHeader)
        {
            if (reader.BaseStream.Position != firstSliceOffset + slicePosition)
                reader.BaseStream.Seek(firstSliceOffset + slicePosition, SeekOrigin.Begin);
            var sliceHeader = ReadSliceHeader(reader);
            var coreDataBlock = ReadCoreDataBlock(reader, compressionHeader);
            var externalDataBlock = ReadExternalDataBlocks(reader, sliceHeader.NumberOfBlocks-1, compressionHeader);
            return new CramSlice(new[] { coreDataBlock }.Concat(externalDataBlock).ToList());
        }

        private CramSliceHeader ReadSliceHeader(BinaryReader reader)
        {
            // Block header
            var blockHeader = ReadBlockHeader(reader);
            if (blockHeader.CompressionMethod != CramBlock.CompressionMethod.Raw)
                throw new NotSupportedException("Found a compressed slice header. Slice headers must not be compressed");

            // Block data
            var blockDataStartPosition = reader.BaseStream.Position;
            var referenceSequenceId = reader.ReadItf8();
            var alignmentStart = reader.ReadItf8();
            var alignmentSpan = reader.ReadItf8();
            var numberOfRecords = reader.ReadItf8();
            var recordCounter = reader.ReadLtf8();
            var numberOfBlocks = reader.ReadItf8();
            var blockContentIds = reader.ReadCramItf8Array();
            var embeddedReferenceBlockContentId = reader.ReadItf8();
            var referenceMd5Checksum = reader.ReadBytes(16);

            var blockDataEndPosition = blockDataStartPosition + blockHeader.UncompressedSize; // True because slice headers are always uncompressed
            var tags = ReadSliceTags(reader, blockDataEndPosition);

            // Block checksum
            var blockChecksum = reader.ReadInt32();

            return new CramSliceHeader(
                referenceSequenceId,
                alignmentStart,
                alignmentSpan,
                numberOfRecords,
                recordCounter,
                numberOfBlocks,
                blockContentIds,
                embeddedReferenceBlockContentId,
                referenceMd5Checksum,
                tags);
        }

        private Dictionary<string, object> ReadSliceTags(BinaryReader reader, long blockDataEndPosition)
        {
            var tags = new Dictionary<string, object>();
            while (reader.BaseStream.Position < blockDataEndPosition)
            {
                var tag = Encoding.ASCII.GetString(reader.ReadBytes(2));
                var type = (char)reader.ReadByte();
                switch (type)
                {
                    case 'A':
                        var c = (char)reader.ReadByte();
                        tags.Add(tag, c);
                        break;
                    case 'B':
                        var array = ReadBArray(reader);
                        tags.Add(tag, array);
                        break;
                    case 'Z':
                    {
                        var str = ReadNullTerminatedString(reader);
                        tags.Add(tag, str);
                        break;
                    }
                    case 'H':
                    {
                        var str = ReadNullTerminatedString(reader);
                        var bytes = ParserHelpers.ParseHexString(str);
                        tags.Add(tag, bytes);
                        break;
                    }
                    case 'c':
                    case 'C':
                    case 's':
                    case 'S':
                    case 'i':
                    case 'I':
                    case 'f':
                        var item = ReadTypedItem(reader, type);
                        tags.Add(tag, item);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tag-type '{type}'. Expected one of: ABZHcCsSiIf");
                }
            }
            return tags;
        }

        private string ReadNullTerminatedString(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            while (true)
            {
                var c = reader.ReadChar();
                if (c == '\0')
                    return stringBuilder.ToString();
                stringBuilder.Append(c);
            }
        }

        private List<object> ReadBArray(BinaryReader reader)
        {
            var itemType = (char)reader.ReadByte();
            var itemCount = reader.ReadUInt32();
            var items = new List<object>();
            for (int i = 0; i < itemCount; i++)
            {
                var item = ReadTypedItem(reader, itemType);
                items.Add(item);
            }
            return items;
        }

        private object ReadTypedItem(BinaryReader reader, char itemType)
        {
            switch (itemType)
            {
                case 'c':
                    return reader.ReadSByte();
                case 'C':
                    return reader.ReadByte();
                case 's':
                    return reader.ReadInt16();
                case 'S':
                    return reader.ReadUInt16();
                case 'i':
                    return reader.ReadInt32();
                case 'I':
                    return reader.ReadUInt32();
                case 'f':
                    return reader.ReadSingle();
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), $"Unknown item type '{itemType}' for B-tag array");
            }
        }

        private CramBlock ReadCoreDataBlock(BinaryReader reader, CramCompressionHeader compressionHeader)
        {
            return ReadBlock(reader, compressionHeader);
        }

        private List<CramBlock> ReadExternalDataBlocks(BinaryReader reader, int numberOfBlocks, CramCompressionHeader compressionHeader)
        {
            return ReadBlocks(reader, numberOfBlocks, compressionHeader);
        }

        private CramCompressionHeader ReadCompressionHeader(BinaryReader reader)
        {
            var blockHeader = ReadBlockHeader(reader);
            var preservationMap = ReadPreservationMap(reader, blockHeader);
            var dataSeriesEncoding = ReadDataSeriesEncoding(reader, blockHeader, preservationMap);
            var tagEncoding = ReadTagEncoding(reader, blockHeader);
            var checksum = reader.ReadInt32();

            return new CramCompressionHeader(preservationMap, dataSeriesEncoding, tagEncoding, checksum);
        }

        private TagEncodingMap ReadTagEncoding(BinaryReader reader, CramBlockHeader blockHeader)
        {
            var sizeInBytes = reader.ReadItf8();
            var numberOfEntries = reader.ReadItf8();
            var tagValueEncodings = new Dictionary<TagId, ICramEncoding<byte[]>>();
            for (int i = 0; i < numberOfEntries; i++)
            {
                var keyItf8Encoded = reader.ReadItf8();
                var tagId = DecodeTagIdForTagEncodingMapEntry(keyItf8Encoded);
                var tagValueEncoding = reader.ReadByteArrayEncoding();
                tagValueEncodings.Add(tagId, tagValueEncoding);
            }
            return new TagEncodingMap(tagValueEncodings);
        }

        private static TagId DecodeTagIdForTagEncodingMapEntry(int keyItf8Encoded)
        {
            var keyBytes = BitConverter.GetBytes(keyItf8Encoded).Reverse().ToArray();
            var key = Encoding.ASCII.GetString(keyBytes, 1, 3);
            var tag = key.Substring(0, 2);
            var valueType = key[2];
            var tagId = new TagId(tag, valueType);
            return tagId;
        }

        private DataSeriesEncodingMap ReadDataSeriesEncoding(BinaryReader reader, CramBlockHeader blockHeader, PreservationMap preservationMap)
        {
            var sizeInBytes = reader.ReadItf8();
            var numberOfEntries = reader.ReadItf8();

            var dataSeriesEncodingMap = new DataSeriesEncodingMap();
            for (int i = 0; i < numberOfEntries; i++)
            {
                var key = Encoding.ASCII.GetString(reader.ReadBytes(2));
                switch (key)
                {
                    case "BF":
                        dataSeriesEncodingMap.BamBitFlagEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "CF":
                        dataSeriesEncodingMap.CramBitFlagEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "RI":
                        dataSeriesEncodingMap.ReferenceIdEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "RL":
                        dataSeriesEncodingMap.ReadLengthsEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "AP":
                        dataSeriesEncodingMap.InSeqPositionsEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "RG":
                        dataSeriesEncodingMap.ReadGroupsEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "RN":
                        dataSeriesEncodingMap.ReadNamesEncoding = reader.ReadByteArrayEncoding();
                        break;
                    case "MF":
                        dataSeriesEncodingMap.NextMateBitFlagEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "NS":
                        dataSeriesEncodingMap.NextFragmentReferenceSequenceIdEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "NP":
                        dataSeriesEncodingMap.NextMateAlignmentStartEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "TS":
                        dataSeriesEncodingMap.TemplateSizeEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "NF":
                        dataSeriesEncodingMap.DistanceToNextFragmentEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "TL":
                        dataSeriesEncodingMap.TagIdEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "FN":
                        dataSeriesEncodingMap.NumberOfReadFeaturesEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "FC":
                        dataSeriesEncodingMap.ReadFeaturesCodesEncoding = reader.ReadByteEncoding();
                        break;
                    case "FP":
                        dataSeriesEncodingMap.InReadPositionsEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "DL":
                        dataSeriesEncodingMap.DeletionLengthEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "BB":
                        dataSeriesEncodingMap.StretchesOfBasesEncoding = reader.ReadByteArrayEncoding();
                        break;
                    case "QQ":
                        dataSeriesEncodingMap.StretchesOfQualityScoresEncoding = reader.ReadByteArrayEncoding();
                        break;
                    case "BS":
                        dataSeriesEncodingMap.BaseSubstitutionCodesEncoding = reader.ReadByteEncoding();
                        break;
                    case "IN":
                        dataSeriesEncodingMap.InsertionEncoding = reader.ReadByteArrayEncoding();
                        break;
                    case "RS":
                        dataSeriesEncodingMap.ReferenceSkipLengthEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "PD":
                        dataSeriesEncodingMap.PaddingEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "HC":
                        dataSeriesEncodingMap.HardClipEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "SC":
                        dataSeriesEncodingMap.SoftClipEncoding = reader.ReadByteArrayEncoding();
                        break;
                    case "MQ":
                        dataSeriesEncodingMap.MappingQualitiesEncoding = reader.ReadIntegerEncoding();
                        break;
                    case "BA":
                        dataSeriesEncodingMap.BasesEncoding = reader.ReadByteEncoding();
                        break;
                    case "QS":
                        dataSeriesEncodingMap.QualityScoresEncoding = reader.ReadByteEncoding();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(key), $"Unknown key '{key}' for data series encoding map");
                }
            }

            return dataSeriesEncodingMap;
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
}