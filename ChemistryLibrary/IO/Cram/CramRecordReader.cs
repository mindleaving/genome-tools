using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramRecordReader
    {
        public List<CramRecord> ReadSliceRecords(
            CramSlice slice,
            IGenomeSequenceAccessor referenceSequenceAccessor)
        {
            var compressionHeader = slice.CompressionHeader;
            var coreDataStream = new BitStream(slice.CoreDataBlock.UncompressedDecodedData);
            var externalBlockStreams = slice.ExternalBlocks.ToDictionary(
                x => x.Header.ContentId, 
                x => new BinaryReader(new MemoryStream(x.UncompressedDecodedData)));

            var records = new List<CramRecord>();
            var lastReadPosition = slice.Header.AlignmentStart;
            for (int recordIndex = 0; recordIndex < slice.Header.NumberOfRecords; recordIndex++)
            {
                var bamBitFlag = DecodeIntegerItem(compressionHeader.DataSeriesEncodingMap.BamBitFlagEncoding, coreDataStream, externalBlockStreams);
                var cramBitFlags = DecodeIntegerItem(compressionHeader.DataSeriesEncodingMap.CramBitFlagEncoding, coreDataStream, externalBlockStreams);
                var (referenceSequenceId,readLength,alignmentPosition,readGroup) = DecodePositions(compressionHeader, slice.Header, coreDataStream, externalBlockStreams, lastReadPosition);
                var readNames = DecodeReadName(compressionHeader, coreDataStream, externalBlockStreams);
                var mateData = DecodeMateData(compressionHeader, coreDataStream, externalBlockStreams, cramBitFlags, ref bamBitFlag);
                var tags = DecodeTags(compressionHeader, coreDataStream, externalBlockStreams);

                GenomeRead read;
                if ((bamBitFlag & 0x4) == 0)
                {
                    read = DecodeMappedRead(
                        compressionHeader,
                        coreDataStream,
                        externalBlockStreams,
                        referenceSequenceId,
                        referenceSequenceAccessor,
                        alignmentPosition,
                        readLength);
                }
                else
                {
                    read = DecodeUnmappedRead(compressionHeader, coreDataStream, externalBlockStreams, readLength);
                }
                var record = new CramRecord(read);
                records.Add(record);
                lastReadPosition = alignmentPosition;
            }
            return records;
        }

        private (int,int,int,int) DecodePositions(
            CramCompressionHeader compressionHeader, 
            CramSliceHeader sliceHeader, 
            BitStream coreDataStream, 
            Dictionary<int, BinaryReader> externalBlockStreams,
            int lastReadPosition)
        {
            var referenceSequenceId = IsMultiReferenceSlice(sliceHeader)
                ? DecodeIntegerItem(compressionHeader.DataSeriesEncodingMap.ReferenceIdEncoding, coreDataStream, externalBlockStreams)
                : sliceHeader.ReferenceSequenceId;
            var readLength = DecodeIntegerItem(compressionHeader.DataSeriesEncodingMap.ReadLengthsEncoding, coreDataStream, externalBlockStreams);
            var alignmentPosition = DecodeIntegerItem(compressionHeader.DataSeriesEncodingMap.InSeqPositionsEncoding, coreDataStream, externalBlockStreams);
            if (compressionHeader.PreservationMap.ApDataSeriesDelta) 
                alignmentPosition += lastReadPosition;
            var readGroup = DecodeIntegerItem(compressionHeader.DataSeriesEncodingMap.ReadGroupsEncoding, coreDataStream, externalBlockStreams);

            return (referenceSequenceId, readLength, alignmentPosition, readGroup);
        }

        private string DecodeReadName(
            CramCompressionHeader compressionHeader, 
            BitStream coreDataStream, 
            Dictionary<int, BinaryReader> externalBlockStreams)
        {
            string readName;
            if (compressionHeader.PreservationMap.ReadNames)
            {
                var readNameBytes = DecodeByteArrayItem(compressionHeader.DataSeriesEncodingMap.ReadNamesEncoding, coreDataStream, externalBlockStreams);
                readName = Encoding.UTF8.GetString(readNameBytes);
            }
            else
            {
                readName = "";
            }
            return readName;
        }

        private object DecodeMateData(
            CramCompressionHeader compressionHeader, 
            BitStream coreDataStream, 
            Dictionary<int, BinaryReader> externalBlockStreams,
            int cramBitFlag,
            ref int bamBitFlag)
        {
            if ((cramBitFlag & 0x2) != 0)
            {
                var mateFlags = DecodeIntegerItem(compressionHeader.DataSeriesEncodingMap.NextMateBitFlagEncoding, coreDataStream, externalBlockStreams);
                if ((mateFlags & 0x1) != 0)
                {
                    bamBitFlag |= 0x20;
                }

                if ((mateFlags & 0x2) != 0)
                {
                    bamBitFlag |= 0x08;
                }
                if(!compressionHeader.PreservationMap.ReadNames)
                {
                    var readNameBytes = DecodeByteArrayItem(compressionHeader.DataSeriesEncodingMap.ReadNamesEncoding, coreDataStream, externalBlockStreams);
                    var readName = Encoding.UTF8.GetString(readNameBytes);
                }

                var mateReferenceId = DecodeIntegerItem(
                    compressionHeader.DataSeriesEncodingMap.NextFragmentReferenceSequenceIdEncoding,
                    coreDataStream,
                    externalBlockStreams);
                var matePosition = DecodeIntegerItem(
                    compressionHeader.DataSeriesEncodingMap.NextMateAlignmentStartEncoding,
                    coreDataStream,
                    externalBlockStreams);
                var templateSize = DecodeIntegerItem(
                    compressionHeader.DataSeriesEncodingMap.TemplateSizeEncoding,
                    coreDataStream,
                    externalBlockStreams);
            }
            else if ((cramBitFlag & 0x4) != 0)
            {
                // TODO: Handle mate downstream
            }

            return null;
        }

        private Dictionary<TagId,object> DecodeTags(
            CramCompressionHeader compressionHeader, 
            BitStream coreDataStream, 
            Dictionary<int, BinaryReader> externalBlockStreams)
        {
            var tagCombinationIndex = DecodeIntegerItem(
                compressionHeader.DataSeriesEncodingMap.TagIdEncoding,
                coreDataStream,
                externalBlockStreams);
            var tagIds = compressionHeader.PreservationMap.TagIdCombinations[tagCombinationIndex];
            var tags = new Dictionary<TagId, object>();
            foreach (var tagId in tagIds)
            {
                var tagEncoding = compressionHeader.TagEncodingMap[tagId];
                var tagValue = DecodeByteArrayItem(tagEncoding, coreDataStream, externalBlockStreams);
                tags.Add(tagId, tagValue);
            }
            return tags;
        }

        private GenomeRead DecodeMappedRead(
            CramCompressionHeader compressionHeader, 
            BitStream coreDataStream, 
            Dictionary<int, BinaryReader> externalBlockStreams,
            int referenceId,
            IGenomeSequenceAccessor referenceSequenceAccessor,
            int readPosition,
            int readLength)
        {
            var referenceSequence = referenceSequenceAccessor.GetSequenceById(referenceId, readPosition, readPosition + readLength - 1);
            var featureNumber = DecodeIntegerItem(
                compressionHeader.DataSeriesEncodingMap.NumberOfReadFeaturesEncoding,
                coreDataStream,
                externalBlockStreams);
            var features = new List<GenomeReadFeature>();
            for (int i = 0; i < featureNumber; i++)
            {
                var feature = DecodeReadFeature(
                    compressionHeader,
                    coreDataStream,
                    externalBlockStreams,
                    referenceSequence);
                features.Add(feature);
            }
            var mappingQuality = DecodeIntegerItem(
                compressionHeader.DataSeriesEncodingMap.MappingQualitiesEncoding,
                coreDataStream,
                externalBlockStreams);
            var qualityScores = DecodeQualityScores(
                compressionHeader, 
                coreDataStream, 
                externalBlockStreams,
                readLength);
            if(qualityScores != null)
                features.Add(new GenomeReadFeature(GenomeSequencePartType.QualityScores, 0, qualityScores: qualityScores.ToCharArray()));
            return GenomeRead.MappedRead(referenceId, readPosition, referenceSequenceAccessor, readLength, features, mappingQuality);
        }

        private GenomeReadFeature DecodeReadFeature(
            CramCompressionHeader compressionHeader, 
            BitStream coreDataStream, 
            Dictionary<int, BinaryReader> externalBlockStreams,
            IGenomeSequence referenceSequence)
        {
            var featureCode = (char)DecodeByteItem(
                compressionHeader.DataSeriesEncodingMap.ReadFeaturesCodesEncoding,
                coreDataStream,
                externalBlockStreams);
            var featurePosition = DecodeIntegerItem(
                compressionHeader.DataSeriesEncodingMap.InReadPositionsEncoding,
                coreDataStream,
                externalBlockStreams);
            switch (featureCode)
            {
                case 'B':
                {
                    var nucleotideByte = DecodeByteItem(
                        compressionHeader.DataSeriesEncodingMap.BasesEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var nucleotide = TranslateBaseByte(nucleotideByte);
                    var qualityScoreByte = DecodeByteItem(
                        compressionHeader.DataSeriesEncodingMap.QualityScoresEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var qualityScore = TranslateQualityScoreByte(qualityScoreByte);
                    return new GenomeReadFeature(
                        GenomeSequencePartType.BaseWithQualityScore,
                        featurePosition,
                        new List<char> { nucleotide },
                        new List<char> { qualityScore });
                }
                case 'X':
                {
                    var substitutionCode = DecodeByteItem(
                        compressionHeader.DataSeriesEncodingMap.BaseSubstitutionCodesEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    char referenceBase;
                    try
                    {
                        referenceBase = referenceSequence.GetBaseAtPosition(featurePosition);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        referenceBase = 'N';
                    }
                    var nucleotide = compressionHeader.PreservationMap.SubstitutionMatrix.Substitute(referenceBase, substitutionCode);
                    return new GenomeReadFeature(GenomeSequencePartType.Bases, featurePosition, new List<char> { nucleotide });
                }
                case 'I':
                {
                    var insertedBaseBytes = DecodeByteArrayItem(
                        compressionHeader.DataSeriesEncodingMap.InsertionEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var insertedBases = insertedBaseBytes.Select(TranslateBaseByte).ToList();
                    return new GenomeReadFeature(GenomeSequencePartType.Insertion, featurePosition, insertedBases);
                }
                case 'S':
                {
                    var softClipBaseBytes = DecodeByteArrayItem(
                        compressionHeader.DataSeriesEncodingMap.SoftClipEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var softClipBases = softClipBaseBytes.Select(TranslateBaseByte).ToList();
                    return new GenomeReadFeature(GenomeSequencePartType.SoftClip, featurePosition, softClipBases);
                }
                case 'H':
                {
                    var hardClipLength = DecodeIntegerItem(
                        compressionHeader.DataSeriesEncodingMap.HardClipEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    return new GenomeReadFeature(GenomeSequencePartType.HardClip, featurePosition, skipLength: hardClipLength);
                }
                case 'P':
                {
                    var padLength = DecodeIntegerItem(
                        compressionHeader.DataSeriesEncodingMap.PaddingEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    return new GenomeReadFeature(GenomeSequencePartType.Padding, featurePosition, skipLength: padLength);
                }
                case 'D':
                {
                    var deletionLength = DecodeIntegerItem(
                        compressionHeader.DataSeriesEncodingMap.DeletionLengthEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    return new GenomeReadFeature(GenomeSequencePartType.Deletion, featurePosition, deletionLength: deletionLength);
                }
                case 'N':
                {
                    var referenceSkipLength = DecodeIntegerItem(
                        compressionHeader.DataSeriesEncodingMap.ReferenceSkipLengthEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    return new GenomeReadFeature(GenomeSequencePartType.ReferenceSkip, featurePosition, skipLength: referenceSkipLength);
                }
                case 'i':
                {
                    var nucleotideByte = DecodeByteItem(
                        compressionHeader.DataSeriesEncodingMap.BasesEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var nucleotide = TranslateBaseByte(nucleotideByte);
                    return new GenomeReadFeature(GenomeSequencePartType.Bases, featurePosition, new List<char>{nucleotide});
                }
                case 'b':
                {
                    var nucleotideBytes = DecodeByteArrayItem(
                        compressionHeader.DataSeriesEncodingMap.StretchesOfBasesEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var nucleotides = nucleotideBytes.Select(TranslateBaseByte).ToList();
                    return new GenomeReadFeature(GenomeSequencePartType.Bases, featurePosition, nucleotides);
                }
                case 'q':
                {
                    var qualityScoreBytes = DecodeByteArrayItem(
                        compressionHeader.DataSeriesEncodingMap.StretchesOfQualityScoresEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var qualityScores = qualityScoreBytes.Select(TranslateQualityScoreByte).ToList();
                    return new GenomeReadFeature(GenomeSequencePartType.QualityScores, featurePosition, qualityScores: qualityScores);
                }
                case 'Q':
                {
                    var qualityScoreByte = DecodeByteItem(
                        compressionHeader.DataSeriesEncodingMap.QualityScoresEncoding,
                        coreDataStream,
                        externalBlockStreams);
                    var qualityScore = TranslateQualityScoreByte(qualityScoreByte);
                    return new GenomeReadFeature(GenomeSequencePartType.QualityScores, featurePosition, qualityScores: new List<char> {qualityScore});
                }
                default:
                    throw new FormatException($"Unknown read feature code '{featureCode}'");
            }
        }

        private char TranslateBaseByte(byte nucleotideByte)
        {
            return (char)nucleotideByte;
        }

        private char TranslateQualityScoreByte(byte qualityScoreByte)
        {
            return (char)qualityScoreByte;
        }

        private GenomeRead DecodeUnmappedRead(
            CramCompressionHeader compressionHeader, 
            BitStream coreDataStream, 
            Dictionary<int, BinaryReader> externalBlockStreams,
            int readLength)
        {
            var bases = new char[readLength];
            for (int i = 0; i < readLength; i++)
            {
                bases[i] = (char)DecodeByteItem(
                    compressionHeader.DataSeriesEncodingMap.BasesEncoding, 
                    coreDataStream, 
                    externalBlockStreams);
            }
            var sequence = new string(bases);
            var qualityScores = DecodeQualityScores(
                compressionHeader, 
                coreDataStream, 
                externalBlockStreams,
                readLength);
            return GenomeRead.UnmappedRead(sequence, qualityScores);
        }

        private string DecodeQualityScores(
            CramCompressionHeader compressionHeader, BitStream coreDataStream, Dictionary<int, BinaryReader> externalBlockStreams,
            int readLength)
        {
            var preserveQualityScores = true; // Specs state that preservation map holds this information, but there's no field documented for that!
            if (!preserveQualityScores) 
                return null;
            var qualityScores = new char[readLength];
            for (int i = 0; i < readLength; i++)
            {
                qualityScores[i] = (char)DecodeByteItem(compressionHeader.DataSeriesEncodingMap.QualityScoresEncoding, coreDataStream, externalBlockStreams);
            }
            return new string(qualityScores);

        }

        private bool IsMultiReferenceSlice(CramSliceHeader sliceHeader)
        {
            return sliceHeader.ReferenceSequenceId == -2;
        }

        private int DecodeIntegerItem(ICramEncoding<int> encoding, BitStream coreDataStream, Dictionary<int, BinaryReader> externalBlockStreams)
        {
            if (encoding.CodecId == Codec.External)
            {
                var externalEncoding = (ExternalCramEncoding<int>)encoding;
                var externalBlock = externalBlockStreams[externalEncoding.BlockContentId];
                return externalBlock.ReadItf8();
            }
            return encoding.Decode(coreDataStream);
        }

        private byte DecodeByteItem(ICramEncoding<byte> encoding, BitStream coreDataStream, Dictionary<int, BinaryReader> externalBlockStreams)
        {
            if (encoding.CodecId == Codec.External)
            {
                var externalEncoding = (ExternalCramEncoding<byte>)encoding;
                var externalBlock = externalBlockStreams[externalEncoding.BlockContentId];
                return externalBlock.ReadByte();
            }
            return encoding.Decode(coreDataStream);
        }

        private byte[] DecodeByteArrayItem(ICramEncoding<byte[]> encoding, BitStream coreDataStream, Dictionary<int, BinaryReader> externalBlockStreams)
        {
            if (encoding.CodecId == Codec.External)
            {
                var externalEncoding = (ExternalCramEncoding<byte[]>)encoding;
                var externalBlock = externalBlockStreams[externalEncoding.BlockContentId];
                return externalBlock.ReadBytes((int)(externalBlock.BaseStream.Length - externalBlock.BaseStream.Position));
            }

            if (encoding.CodecId == Codec.ByteArrayStop)
            {
                var byteArrayStopEncoding = (ByteArrayStopCramEncoding)encoding;
                var externalBlock = externalBlockStreams[byteArrayStopEncoding.ExternalBlockContentId];
                var bytes = new List<byte>();
                byte b;
                while ((b = externalBlock.ReadByte()) != byteArrayStopEncoding.StopValue)
                {
                    bytes.Add(b);
                }
                return bytes.ToArray();
            }

            if (encoding.CodecId == Codec.ByteArrayLength)
            {
                var byteLengthEncoding = (ByteArrayLengthCramEncoding)encoding;
                var arrayLength = DecodeIntegerItem(byteLengthEncoding.ArrayLengthEncoding, coreDataStream, externalBlockStreams);
                var bytes = new byte[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                {
                    bytes[i] = DecodeByteItem(byteLengthEncoding.ValuesEncoding, coreDataStream, externalBlockStreams);
                }
                return bytes;
            }

            return encoding.Decode(coreDataStream);
        }
    }
}
