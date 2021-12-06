using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenomeReadFormatter
    {
        public string GetReadSequence(
            List<GenomeReadFeature> readFeatures, 
            int readLength,
            string referenceSequence)
        {
            var sequence = referenceSequence;
            var deletions = readFeatures.Where(x => x.Type == GenomeSequencePartType.Deletion);
            var insertions = readFeatures.Where(x => x.Type == GenomeSequencePartType.Insertion);
            foreach (var insertDelete in insertions.Concat(deletions).OrderBy(x => x.InReadPosition))
            {
                switch (insertDelete.Type)
                {
                    case GenomeSequencePartType.Insertion:
                    {
                        var bases = new string(insertDelete.Sequence.ToArray());
                        sequence = sequence.Insert(insertDelete.InReadPosition, bases);
                        break;
                    }
                    case GenomeSequencePartType.Deletion:
                        sequence = sequence.Remove(insertDelete.InReadPosition, insertDelete.DeletionLength.Value);
                        break;
                }
            }

            if (sequence.Length != readLength)
                throw new Exception("Sequence hasn't the expected length. Are all relevant features taken into account?");

            var sequenceArray = sequence.ToCharArray();

            foreach (var feature in readFeatures)
            {
                switch (feature.Type)
                {
                    case GenomeSequencePartType.Bases:
                    case GenomeSequencePartType.SoftClip:
                        Copy(feature.Sequence, 0, sequenceArray, feature.InReadPosition, feature.Sequence.Count);
                        break;
                    case GenomeSequencePartType.ReferenceSkip:
                    case GenomeSequencePartType.HardClip:
                    case GenomeSequencePartType.Padding:
                    case GenomeSequencePartType.QualityScores:
                        if(feature.Sequence != null)
                        {
                            Copy(feature.Sequence, 0, sequenceArray, feature.InReadPosition, feature.Sequence.Count);
                        }
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                    case GenomeSequencePartType.Substitution:
                        sequenceArray[feature.InReadPosition] = feature.Sequence[0];
                        break;
                    case GenomeSequencePartType.Insertion:
                    case GenomeSequencePartType.Deletion:
                        // Have already been handled
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return new string(sequenceArray);
        }

        public string GetReferenceAlignedSequence(
            List<GenomeReadFeature> readFeatures,
            string referenceSequence)
        {
            var sequence = referenceSequence.ToCharArray();
            var deletionOffset = 0;
            var insertionOffset = 0;
            foreach (var feature in readFeatures)
            {
                var referencePosition = feature.InReadPosition + deletionOffset - insertionOffset;
                switch (feature.Type)
                {
                    case GenomeSequencePartType.Bases:
                    case GenomeSequencePartType.SoftClip:
                        Copy(feature.Sequence, 0, sequence, referencePosition, feature.Sequence.Count);
                        break;
                    case GenomeSequencePartType.Insertion:
                        insertionOffset += feature.Sequence.Count;
                        break;
                    case GenomeSequencePartType.QualityScores:
                        if(feature.Sequence != null)
                        {
                            Copy(feature.Sequence, 0, sequence, referencePosition, feature.Sequence.Count);
                        }
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                    case GenomeSequencePartType.Substitution:
                        sequence[referencePosition] = feature.Sequence[0];
                        break;
                    case GenomeSequencePartType.Deletion:
                        for (int i = 0; i < feature.DeletionLength; i++)
                        {
                            if(referencePosition+i < 0 || referencePosition+i >= sequence.Length)
                                continue;
                            sequence[referencePosition+i] = '-';
                        }
                        deletionOffset += feature.DeletionLength.Value;
                        break;
                    case GenomeSequencePartType.ReferenceSkip:
                    case GenomeSequencePartType.HardClip:
                    case GenomeSequencePartType.Padding:
                        if (feature.Sequence != null)
                        {
                            Copy(feature.Sequence, 0, sequence, referencePosition, feature.Sequence.Count);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return new string(sequence);
        }

        public string GetReadQualityScores(List<GenomeReadFeature> readFeatures, int readLength)
        {
            var qualityScores = new char[readLength];
            Array.Fill(qualityScores, '!');
            foreach (var feature in readFeatures)
            {
                switch (feature.Type)
                {
                    case GenomeSequencePartType.QualityScores:
                        Copy(feature.QualityScores, 0, qualityScores, feature.InReadPosition, feature.QualityScores.Count);
                        break;
                    case GenomeSequencePartType.Bases:
                    case GenomeSequencePartType.SoftClip:
                    case GenomeSequencePartType.Insertion:
                    case GenomeSequencePartType.ReferenceSkip:
                    case GenomeSequencePartType.HardClip:
                    case GenomeSequencePartType.Padding:
                        if(feature.QualityScores != null)
                        {
                            Copy(feature.QualityScores, 0, qualityScores, feature.InReadPosition, feature.QualityScores.Count);
                        }
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                        qualityScores[feature.InReadPosition] = feature.QualityScores[0];
                        break;
                    case GenomeSequencePartType.Substitution:
                        if(feature.QualityScores != null)
                        {
                            qualityScores[feature.InReadPosition] = feature.QualityScores[0];
                        }
                        break;
                    case GenomeSequencePartType.Deletion:
                        // Ignore deletes
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return new string(qualityScores);
        }

        public string GetReferenceAlignedQualityScores(List<GenomeReadFeature> readFeatures, int readLength)
        {
            var deletionLength = readFeatures
                .Where(x => x.Type == GenomeSequencePartType.Deletion)
                .Select(x => x.DeletionLength.Value)
                .Sum();
            var insertionLength = readFeatures
                .Where(x => x.Type == GenomeSequencePartType.Insertion)
                .Select(x => x.Sequence.Count)
                .Sum();
            var qualityScores = new char[readLength+deletionLength-insertionLength];
            Array.Fill(qualityScores, '!');
            var deletionOffset = 0;
            var insertionOffset = 0;
            foreach (var feature in readFeatures)
            {
                var referencePosition = feature.InReadPosition + deletionOffset - insertionOffset;
                switch (feature.Type)
                {
                    case GenomeSequencePartType.QualityScores:
                        Copy(feature.QualityScores, 0, qualityScores, referencePosition, feature.QualityScores.Count);
                        break;
                    case GenomeSequencePartType.Bases:
                    case GenomeSequencePartType.SoftClip:
                        if(feature.QualityScores != null)
                        {
                            Copy(feature.QualityScores, 0, qualityScores, referencePosition, feature.QualityScores.Count);
                        }
                        break;
                    case GenomeSequencePartType.Insertion:
                        insertionOffset = feature.Sequence.Count;
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                        qualityScores[referencePosition] = feature.QualityScores[0];
                        break;
                    case GenomeSequencePartType.Substitution:
                        if(feature.QualityScores != null)
                        {
                            qualityScores[referencePosition] = feature.QualityScores[0];
                        }
                        break;
                    case GenomeSequencePartType.Deletion:
                        deletionOffset += feature.DeletionLength.Value;
                        break;
                    case GenomeSequencePartType.ReferenceSkip:
                    case GenomeSequencePartType.HardClip:
                    case GenomeSequencePartType.Padding:
                        if (feature.QualityScores != null)
                        {
                            Copy(feature.QualityScores, 0, qualityScores, referencePosition, feature.QualityScores.Count);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return new string(qualityScores);
        }

        private static void Copy(
            IList<char> source, int sourceOffset, IList<char> target,
            int targetOffset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                var sourceIndex = sourceOffset + i;
                var targetIndex = targetOffset + i;
                if(sourceIndex < 0 || sourceIndex >= source.Count)
                    continue;
                if(targetIndex < 0 || targetIndex >= target.Count)
                    continue;
                target[targetIndex] = source[sourceIndex];
            }
        }

    }
}
