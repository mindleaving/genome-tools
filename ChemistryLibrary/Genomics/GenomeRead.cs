﻿using System;
using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.IO;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenomeRead : IGenomeSequence
    {
        private readonly string readSequence;
        private readonly string referenceAlignedSequence;
        private readonly string readQualityScores;
        private readonly string referenceAlignedQualityScores;

        public string Id { get; }
        public IReadOnlyList<GenomeReadFeature> ReadFeatures { get; }
        public int Length => readSequence.Length;
        public bool IsMapped { get; }
        public int? MappingQuality { get; }
        public int? ReferenceId { get; }
        public int? ReferenceStartIndex { get; }
        public int? ReferenceEndIndex { get; }

        private GenomeRead(
            string id, 
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
            Id = id;
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
            IGenomeSequenceAccessor referenceAccessor,
            int readLength,
            List<GenomeReadFeature> readFeatures, 
            int mappingQuality,
            string id = null)
        {
            var insertionsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.Insertion).Sum(x => x.Sequence.Count);
            var softClipsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.SoftClip).Sum(x => x.Sequence.Count);
            var deletionsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.Deletion).Sum(x => x.DeletionLength.Value);
            var skipsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.ReferenceSkip).Sum(x => x.SkipLength.Value);
            var referenceEndIndex = referenceStartIndex + readLength - insertionsLength - softClipsLength + deletionsLength + skipsLength - 1;
            var referenceSequence = referenceAccessor.GetSequenceById(referenceId, referenceStartIndex, referenceEndIndex).GetSequence();

            readFeatures.Sort((a,b) => a.InReadPosition.CompareTo(b.InReadPosition));

            var readFormatter = new GenomeReadFormatter();
            var readSequence = readFormatter.GetReadSequence(readFeatures, readLength, referenceSequence);
            var readQualityScores = readFormatter.GetReadQualityScores(readFeatures, readLength);
            var referenceAlignedSequence = readFormatter.GetReferenceAlignedSequence(readFeatures, referenceSequence);
            var referenceAlignedQualityScores = readFormatter.GetReferenceAlignedQualityScores(readFeatures, readLength);
            return new GenomeRead(
                id,
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

        public static GenomeRead UnmappedRead(
            IEnumerable<char> sequence,
            IEnumerable<char> qualityScores,
            string id = null,
            int? referenceId = null,
            int? referenceStartIndex = null)
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
                id,
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

        public char GetBaseAtReferencePosition(int referencePosition)
        {
            if(!IsMapped)
                throw new InvalidOperationException("Genome read is not mapped to reference");
            return referenceAlignedSequence[referencePosition-ReferenceStartIndex.Value];
        }

        public char GetQualityScoreAtPosition(int inReadPosition)
        {
            if (inReadPosition >= Length)
                throw new IndexOutOfRangeException($"Position was outside of read. Position: {inReadPosition}. Length: {Length}");
            return readQualityScores[inReadPosition];
        }

        public char GetQualityScoreAtReferencePosition(int referencePosition)
        {
            if(!IsMapped)
                throw new InvalidOperationException("Genome read is not mapped to reference");
            return referenceAlignedQualityScores[referencePosition-ReferenceStartIndex.Value];
        }
    }
}