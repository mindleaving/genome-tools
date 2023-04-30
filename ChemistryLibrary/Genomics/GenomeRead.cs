using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.IO;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenomeRead : IGenomeSequence
    {
        private readonly string readSequence;
        private readonly string referenceAlignedSequence;
        private readonly string readQualityScores;
        private readonly string referenceAlignedQualityScores;
        private readonly Dictionary<int, int> referenceIndexToReadIndexMap;

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
            Dictionary<int,int> referenceIndexToReadIndexMap,
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
            this.referenceIndexToReadIndexMap = referenceIndexToReadIndexMap;
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
            var readFormatter = new GenomeReadFormatter();
            readFeatures = readFormatter.SortReadFeatures(readFeatures);

            var insertionsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.Insertion).Sum(x => x.Sequence.Count);
            var softClipsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.SoftClip).Sum(x => x.Sequence.Count);
            var deletionsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.Deletion).Sum(x => x.DeletionLength.Value);
            var skipsLength = readFeatures.Where(x => x.Type == GenomeSequencePartType.ReferenceSkip).Sum(x => x.SkipLength.Value);
            var referenceEndIndex = referenceStartIndex + readLength - insertionsLength - softClipsLength + deletionsLength + skipsLength - 1;
            var referenceSequence = referenceAccessor.GetSequenceById(referenceId, referenceStartIndex, referenceEndIndex).GetSequence();
            var referenceIndexMap = MapReferenceIndexToReadIndex(readFeatures, referenceStartIndex);

            var readSequence = readFormatter.GetReadSequence(readFeatures, readLength, referenceSequence, isReadFeaturesSorted: true);
            var readQualityScores = readFormatter.GetReadQualityScores(readFeatures, readLength, isReadFeaturesSorted: true);
            var referenceAlignedSequence = readFormatter.GetReferenceAlignedSequence(readFeatures, referenceSequence, isReadFeaturesSorted: true);
            var referenceAlignedQualityScores = readFormatter.GetReferenceAlignedQualityScores(readFeatures, readLength, isReadFeaturesSorted: true);
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
                referenceIndexMap,
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

        private static Dictionary<int, int> MapReferenceIndexToReadIndex(
            List<GenomeReadFeature> readFeatures,
            int referenceStartIndex)
        {
            var indexMap = new Dictionary<int, int>();
            var referenceIndex = referenceStartIndex;
            foreach (var feature in readFeatures)
            {
                switch (feature.Type)
                {
                    case GenomeSequencePartType.Bases:
                        for (int baseIndex = 0; baseIndex < feature.Sequence.Count; baseIndex++)
                        {
                            var readIndex = feature.InReadPosition + baseIndex;
                            indexMap[referenceIndex] = readIndex;
                            referenceIndex++;
                        }
                        break;
                    case GenomeSequencePartType.QualityScores:
                        // Do nothing
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                    case GenomeSequencePartType.Substitution:
                    {
                        var readIndex = feature.InReadPosition;
                        indexMap[referenceIndex] = readIndex;
                        referenceIndex++;
                        break;
                    }
                    case GenomeSequencePartType.Insertion:
                    {
                        var readIndex = feature.InReadPosition;
                        indexMap[referenceIndex] = readIndex;
                        //referenceIndex++;
                        break;
                    }
                    case GenomeSequencePartType.Deletion:
                    {
                        var readIndex = feature.InReadPosition;
                        for (int deletionIndex = 0; deletionIndex < feature.DeletionLength!.Value; deletionIndex++)
                        {
                            indexMap[referenceIndex] = readIndex;
                            referenceIndex++;
                        }
                        break;
                    }
                    case GenomeSequencePartType.ReferenceSkip:
                    {
                        var readIndex = feature.InReadPosition;
                        for (int deletionIndex = 0; deletionIndex < feature.SkipLength!.Value; deletionIndex++)
                        {
                            indexMap[referenceIndex] = readIndex;
                            referenceIndex++;
                        }
                        break;
                    }
                    case GenomeSequencePartType.SoftClip:
                    case GenomeSequencePartType.HardClip:
                    case GenomeSequencePartType.Padding:
                        // Nothing to do
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return indexMap;
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

        public IEnumerable<GenomeReadFeature> GetFeaturesAtReferencePosition(
            int referenceIndex)
        {
            if (!referenceIndexToReadIndexMap.ContainsKey(referenceIndex))
                return Enumerable.Empty<GenomeReadFeature>();
            var readIndex = referenceIndexToReadIndexMap[referenceIndex];
            return ReadFeatures.Where(
                x =>
                {
                    switch (x.Type)
                    {
                        case GenomeSequencePartType.Bases:
                        case GenomeSequencePartType.QualityScores:
                        case GenomeSequencePartType.SoftClip:
                        case GenomeSequencePartType.Padding:
                            return readIndex.IsBetween(x.InReadPosition, x.InReadPosition + x.Sequence.Count - 1);
                        case GenomeSequencePartType.BaseWithQualityScore:
                        case GenomeSequencePartType.Substitution:
                        case GenomeSequencePartType.Deletion:
                        case GenomeSequencePartType.ReferenceSkip:
                        case GenomeSequencePartType.HardClip:
                            return x.InReadPosition == readIndex;
                        case GenomeSequencePartType.Insertion:
                            return x.InReadPosition == readIndex + 1;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });
        }
    }
}