using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeReadFormatter
    {
        
        public string GetReadSequence(List<GenomeReadFeature> readFeatures, int readLength)
        {
            var sequence = new char[readLength];
            Array.Fill(sequence, '\0');
            foreach (var feature in readFeatures)
            {
                switch (feature.Type)
                {
                    case GenomeSequencePartType.Bases:
                    case GenomeSequencePartType.SoftClip:
                    case GenomeSequencePartType.Insertion:
                        Copy(feature.Sequence, 0, sequence, feature.InReadPosition, feature.Sequence.Count);
                        break;
                    case GenomeSequencePartType.ReferenceSkip:
                    case GenomeSequencePartType.HardClip:
                    case GenomeSequencePartType.Padding:
                    case GenomeSequencePartType.QualityScores:
                        if(feature.Sequence != null)
                        {
                            Copy(feature.Sequence, 0, sequence, feature.InReadPosition, feature.Sequence.Count);
                        }
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                    case GenomeSequencePartType.Substitution:
                        sequence[feature.InReadPosition] = feature.Sequence[0];
                        break;
                    case GenomeSequencePartType.Deletion:
                        // Ignore
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return new string(sequence);
        }

        public string GetReferenceAlignedSequence(List<GenomeReadFeature> readFeatures, int readLength)
        {
            var deletionLength = readFeatures
                .Where(x => x.Type == GenomeSequencePartType.Deletion)
                .Select(x => x.DeletionLength.Value)
                .Sum();
            var insertionLength = readFeatures
                .Where(x => x.Type == GenomeSequencePartType.Insertion)
                .Select(x => x.Sequence.Count)
                .Sum();
            var sequence = new char[readLength + deletionLength - insertionLength];
            Array.Fill(sequence, '-');
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
            Array.Fill(qualityScores, '\0');
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
            Array.Fill(qualityScores, '\0');
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
