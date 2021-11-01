using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.IO.Cram.Index;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramGenomeSequenceAlignmentAccessor : IGenomeAlignmentAccessor
    {
        private readonly string alignmentFilePath;
        private readonly ReferenceSequenceMap referenceSequenceMap;
        private readonly string alignmentIndexFilePath;
        private CramIndex alignmentIndex;
        private readonly GenomeSequenceAccessor referenceSequenceAccessor;

        public CramGenomeSequenceAlignmentAccessor(
            string alignmentFilePath, 
            string referenceSequenceFilePath,
            ReferenceSequenceMap referenceSequenceMap)
        {
            this.alignmentFilePath = alignmentFilePath;
            this.referenceSequenceMap = referenceSequenceMap;
            alignmentIndexFilePath = alignmentFilePath + ".crai";
            referenceSequenceAccessor = new GenomeSequenceAccessor(referenceSequenceFilePath, referenceSequenceMap);
        }

        public IGenomeSequence GetReferenceSequence(string chromosome, int startIndex, int endIndex)
        {
            return referenceSequenceAccessor.GetSequenceByName(chromosome, startIndex, endIndex);
        }

        public IGenomeSequence GetAlignmentSequence(string chromosome, int startIndex, int endIndex)
        {
            var alignment = GetAlignment(chromosome, startIndex, endIndex);
            return alignment.AlignmentSequence;
        }

        public GenomeSequenceAlignment GetAlignment(string chromosome, int startIndex, int endIndex)
        {
            var referenceSequence = GetReferenceSequence(chromosome, startIndex, endIndex);
            var reads = GetReadsInRange(chromosome, startIndex, endIndex);
            var consensusSequenceBuilder = new GenomeConsensusSequenceBuilder();
            var consensusSequence = consensusSequenceBuilder.Build(reads, chromosome);
            return new GenomeSequenceAlignment(
                chromosome,
                startIndex,
                endIndex,
                referenceSequence,
                consensusSequence,
                reads);
        }

        private List<GenomeRead> GetReadsInRange(string chromosome, int startIndex, int endIndex)
        {
            var referenceId = referenceSequenceMap.GetIndexFromSequenceName(chromosome);
            var overlappingSlices = GetOverlappingSlices(referenceId, startIndex, endIndex);
            var reader = new CramRecordReader();
            var readsInRange = new List<GenomeRead>();
            foreach (var slice in overlappingSlices)
            {
                var sliceRecords = reader.ReadSliceRecords(slice, referenceSequenceAccessor);
                var overlappingReads = sliceRecords
                    .Select(x => x.Read)
                    .Where(x => x.ReferenceStartIndex.HasValue)
                    .Where(x => !(endIndex < x.ReferenceStartIndex.Value || startIndex >= x.ReferenceStartIndex.Value+x.Length))
                    .ToList();
                readsInRange.AddRange(overlappingReads);
            }
            return readsInRange;
        }

        private List<CramSlice> GetOverlappingSlices(int sequenceId, int startIndex, int endIndex)
        {
            if (alignmentIndex == null)
                LoadAlignmentIndex();
            var matchingIndexEntries = alignmentIndex.GetEntriesForReferenceSequence(sequenceId)
                .Where(x => Overlaps(x, startIndex, endIndex))
                .ToList();
            var reader = new CramBinaryReader(alignmentFilePath);
            reader.CheckFileFormat();
            var sliceReader = new CramSliceReader();
            return matchingIndexEntries
                .Select(indexEntry => sliceReader.Read(reader, indexEntry))
                .ToList();
        }

        private bool Overlaps(CramIndexEntry indexEntry, int startIndex, int endIndex)
        {
            var entryStartIndex = indexEntry.AlignmentStart;
            var entryEndIndex = indexEntry.AlignmentStart + indexEntry.AlignmentSpan - 1;

            if (startIndex > entryEndIndex)
                return false;
            if (entryStartIndex > endIndex)
                return false;
            return true;
        }

        private void LoadAlignmentIndex()
        {
            var cramIndexReader = new CramIndexLoader();
            alignmentIndex = cramIndexReader.Load(alignmentIndexFilePath);
        }
    }
}
