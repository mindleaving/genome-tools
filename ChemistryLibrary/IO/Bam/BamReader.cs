using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Sam;
using ICSharpCode.SharpZipLib.GZip;

namespace GenomeTools.ChemistryLibrary.IO.Bam
{
    public class BamReader
    {
        public List<GenomeRead> Load(
            string bamFilePath,
            string referenceFilePath)
        {
            var reads = new List<GenomeRead>();
            Load(bamFilePath, referenceFilePath, reads.Add);
            return reads;
        }

        public void Load(
            string bamFilePath,
            string referenceFilePath,
            Action<GenomeRead> action)
        {
            using var fileStream = File.OpenRead(bamFilePath);
            using var memoryStream = new MemoryStream();
            GZip.Decompress(fileStream, memoryStream, false);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using var reader = new BinaryReader(memoryStream);
            var magic = reader.ReadBytes(4);
            if (magic[0] != 'B' || magic[1] != 'A' || magic[2] != 'M' || magic[3] != 1)
                throw new IOException("Invalid BAM format");
            var headerLength = reader.ReadUInt32();
            var headerParser = new SamHeaderParser();
            var header = new string(reader.ReadChars((int)headerLength))
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(headerParser.Parse)
                .ToList();
            var referenceSequenceCount = reader.ReadUInt32();
            var referenceNameToIndexMap = new Dictionary<string, int>();
            var referenceIndexToNameMap = new Dictionary<int, string>();
            for (int referenceIndex = 0; referenceIndex < referenceSequenceCount; referenceIndex++)
            {
                var referenceSequenceNameLength = (int)reader.ReadUInt32();
                var referenceSequenceName = ReadNullTerminatedString(reader, referenceSequenceNameLength);
                var referenceSequenceLength = reader.ReadUInt32();
                referenceNameToIndexMap.Add(referenceSequenceName, referenceIndex);
                referenceIndexToNameMap.Add(referenceIndex, referenceSequenceName);
            }

            var referenceOrder = new ReferenceSequenceMap(referenceNameToIndexMap, referenceIndexToNameMap);

            var cigarConverter = new BamCigarToReadFeatureConverter();
            var bamTagReader = new BamTagReader();
            var referenceAccessor = new GenomeSequenceAccessor(referenceFilePath, referenceOrder);
            while (memoryStream.Position < memoryStream.Length)
            {
                var blockSize = reader.ReadUInt32();
                var blockStartPostion = memoryStream.Position; 
                var referenceSequenceId = reader.ReadInt32();
                var isMapped = referenceSequenceId >= 0;
                var referenceStartIndex = reader.ReadInt32();
                var readNameLength = reader.ReadByte();
                var mappingQuality = reader.ReadByte();
                var indexBin = reader.ReadUInt16();
                var cigarOperationCount = reader.ReadUInt16();
                var flag = reader.ReadUInt16();
                var sequenceLength = (int)reader.ReadUInt32();
                var nextReferenceSequenceId = reader.ReadInt32();
                var nextReferenceStartIndex = reader.ReadInt32();
                var templateLength = reader.ReadInt32();
                var readName = ReadNullTerminatedString(reader, readNameLength);
                var cigar = ReadCigar(reader, cigarOperationCount);
                var sequence = ReadSequence(reader, sequenceLength);
                var qualityScores = reader.ReadChars(sequenceLength).Select(x => (char)(x + 33)).ToList();
                var readFeatures = cigarConverter.Convert(cigar, sequence, qualityScores);
                if(isMapped 
                   && readFeatures
                       .Where(x => x.Type.InSet(GenomeSequencePartType.Bases, GenomeSequencePartType.Insertion, GenomeSequencePartType.SoftClip))
                       .Sum(x => x.Sequence.Count) != sequenceLength)
                    Debugger.Break();

                var currentBlockPosition = 32 + readNameLength + 4 * cigarOperationCount + (sequenceLength + 1) / 2 + sequenceLength;
                var tags = new Dictionary<string, object>();
                while (currentBlockPosition < blockSize)
                {
                    var tag = new string(reader.ReadChars(2)).ToUpper();
                    var tagValue = bamTagReader.ReadTagValue(reader);
                    tags.Add(tag, tagValue.Value);
                    currentBlockPosition += 2 + tagValue.BytesRead;
                }
                var read = isMapped
                ? GenomeRead.MappedRead(
                    referenceSequenceId,
                    referenceStartIndex,
                    referenceAccessor,
                    sequenceLength,
                    readFeatures,
                    mappingQuality,
                    id: readName)
                : GenomeRead.UnmappedRead(
                    sequence,
                    qualityScores,
                    id: readName);
                action(read);
            }
        }

        private static string ReadNullTerminatedString(
            BinaryReader reader,
            int referenceSequenceNameLength)
        {
            return new string(reader.ReadChars(referenceSequenceNameLength), 0, referenceSequenceNameLength - 1);
        }

        private IList<char> ReadSequence(
            BinaryReader reader,
            int sequenceLength)
        {
            var bytes = reader.ReadBytes((sequenceLength + 1) / 2);
            var bases = new char[sequenceLength];
            var baseMap = "=ACMGRSVTWYHKDBN";
            for (int baseIndex = 0; baseIndex < sequenceLength; baseIndex++)
            {
                var b = bytes[baseIndex / 2];
                byte baseCode;
                if (!baseIndex.IsOdd())
                {
                    // First 4 bits
                    baseCode = (byte)(b >> 4);
                }
                else
                {
                    // Last 4 bits
                    baseCode = (byte)(b & 0x0f);
                }
                var mappedBase = baseMap[baseCode];
                bases[baseIndex] = mappedBase;
            }
            return bases;
        }

        private uint[] ReadCigar(
            BinaryReader reader,
            ushort cigarOperationCount)
        {
            var cigarOperations = new uint[cigarOperationCount];
            for (int operationIndex = 0; operationIndex < cigarOperationCount; operationIndex++)
            {
                cigarOperations[operationIndex] = reader.ReadUInt32();
            }
            return cigarOperations;
        }
    }
}
