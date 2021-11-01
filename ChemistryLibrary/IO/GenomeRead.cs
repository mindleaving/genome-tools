using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeRead : IGenomeSequence
    {
        private readonly string readSequence;
        private readonly string referenceAlignedSequence;
        private readonly string readQualityScores;
        private readonly string referenceAlignedQualityScores;

        public IReadOnlyList<GenomeReadFeature> ReadFeatures { get; }
        public int Length => readSequence.Length;
        public bool IsMapped { get; }
        public int? MappingQuality { get; }
        public int? ReferenceId { get; }
        public int? ReferenceStartIndex { get; }
        public int? ReferenceEndIndex { get; }

        private GenomeRead(
            bool isMapped, 
            int? referenceId, 
            int? referenceStartIndex,
            int? referenceEndIndex,
            IReadOnlyList<GenomeReadFeature> readFeatures, 
            string readSequence, 
            string readQualityScores,
            string referenceAlignedSequence,
            string referenceAlignedQualityScores,
            int? mappingQuality = null)
        {
            if (isMapped && (!referenceId.HasValue || !referenceStartIndex.HasValue))
            {
                throw new ArgumentException("Mapped genome reads must have a reference ID and position");
            }

            if (referenceStartIndex.HasValue && !referenceEndIndex.HasValue)
            {
                throw new ArgumentNullException(nameof(referenceEndIndex), "If a genome read has a start index it must have an end index as well");
            }

            this.readSequence = readSequence;
            this.readQualityScores = readQualityScores;
            this.referenceAlignedSequence = referenceAlignedSequence;
            this.referenceAlignedQualityScores = referenceAlignedQualityScores;
            IsMapped = isMapped;
            ReferenceId = referenceId;
            ReferenceStartIndex = referenceStartIndex;
            ReferenceEndIndex = referenceEndIndex;
            ReadFeatures = readFeatures;
            MappingQuality = mappingQuality;
        }

        public static GenomeRead MappedRead(
            int referenceId, 
            int referenceStartIndex, 
            int readLength,
            List<GenomeReadFeature> readFeatures, 
            int mappingQuality)
        {
            readFeatures.Sort((a,b) => a.InReadPosition.CompareTo(b.InReadPosition));

            var readFormatter = new GenomeReadFormatter();
            var readSequence = readFormatter.GetReadSequence(readFeatures, readLength);
            var readQualityScores = readFormatter.GetReadQualityScores(readFeatures, readLength);
            var referenceAlignedSequence = readFormatter.GetReferenceAlignedSequence(readFeatures, readLength);
            var referenceAlignedQualityScores = readFormatter.GetReferenceAlignedQualityScores(readFeatures, readLength);
            var insertionsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.Insertion).Sum(x => x.Sequence.Count);
            var deletionsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.Deletion).Sum(x => x.DeletionLength.Value);
            var referenceEndIndex = referenceStartIndex + readLength - insertionsLength + deletionsLength - 1;
            return new GenomeRead(
                true,
                referenceId,
                referenceStartIndex,
                referenceEndIndex,
                readFeatures,
                readSequence,
                readQualityScores,
                referenceAlignedSequence,
                referenceAlignedQualityScores,
                mappingQuality);
        }

        public static GenomeRead UnmappedRead(IEnumerable<char> sequence, IEnumerable<char> qualityScores, int? referenceId = null, int? referenceStartIndex = null)
        {
            var readSequence = new string(sequence.ToArray());
            var readQualityScores = qualityScores != null ? new string(qualityScores.ToArray()) : null;
            var features = new List<GenomeReadFeature>
            {
                new(GenomeSequencePartType.Bases, 0, readSequence.ToList(), readQualityScores?.ToList())
            };
            int? referenceEndIndex = null;
            if (referenceStartIndex.HasValue)
                referenceEndIndex = referenceStartIndex + readSequence.Length - 1;
            return new GenomeRead(
                false,
                referenceId,
                referenceStartIndex,
                referenceEndIndex,
                features,
                readSequence,
                readQualityScores,
                null,
                null);
        }

        public string GetSequence()
        {
            return readSequence;
        }

        public string GetReferenceAlignedSequence()
        {
            if(!IsMapped)
                throw new InvalidOperationException("Reference aligned sequence is not supported for unmapped reads, even if they are placed. Use GetSequence() instead");
            return referenceAlignedSequence;
        }

        public string GetQualityScores()
        {
            return readQualityScores;
        }

        public string GetReferenceQualityScores()
        {
            if(!IsMapped)
                throw new InvalidOperationException("Reference aligned quality scores are not supported for unmapped reads, even if they are placed. Use GetQualityScores() instead");
            return referenceAlignedQualityScores;
        }

        public char GetBaseAtPosition(int inReadPosition)
        {
            if (inReadPosition >= Length)
                throw new IndexOutOfRangeException($"Position was outside of read. Position: {inReadPosition}. Length: {Length}");
            return readSequence[inReadPosition];
        }
    }
}