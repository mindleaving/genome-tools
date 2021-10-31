using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramCompressionHeaderReader
    {
        public CramCompressionHeader Read(CramBinaryReader reader)
        {
            var blockHeaderReader = new CramBlockHeaderReader();
            var blockHeader = blockHeaderReader.Read(reader);
            var preservationMap = ReadPreservationMap(reader, blockHeader);
            var dataSeriesEncoding = ReadDataSeriesEncoding(reader, blockHeader, preservationMap);
            var tagEncoding = ReadTagEncoding(reader, blockHeader);
            var checksum = reader.ReadInt32();

            return new CramCompressionHeader(preservationMap, dataSeriesEncoding, tagEncoding, checksum);
        }

        private PreservationMap ReadPreservationMap(CramBinaryReader reader, CramBlockHeader blockHeader)
        {
            var sizeInBytes = reader.ReadItf8();
            var numberOfEntries = reader.ReadItf8();
            var readNames = true;
            var apDataSeriesDelta = true;
            var referenceRequired = true;
            BaseSubstitutionMatrix substitutionMatrix = null;
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
                        substitutionMatrix = new BaseSubstitutionMatrix(reader.ReadBytes(5));
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

        private Dictionary<TagId,ICramEncoding<byte[]>> ReadTagEncoding(CramBinaryReader reader, CramBlockHeader blockHeader)
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
            return tagValueEncodings;
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

        private DataSeriesEncodingMap ReadDataSeriesEncoding(CramBinaryReader reader, CramBlockHeader blockHeader, PreservationMap preservationMap)
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
                        // See: https://github.com/samtools/hts-specs/issues/598
                        reader.ReadAndDiscardUnknownEncoding();
                        break;
                }
            }

            return dataSeriesEncodingMap;
        }
    }
}
