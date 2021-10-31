using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeRead : IGenomeSequence
    {
        private readonly string compiledSequence;
        private readonly string compiledQualityScores;

        private IReadOnlyList<GenomeReadFeature> ReadFeatures { get; }

        public int? ReferenceId { get; }
        public int? ReadPosition { get; }
        public bool IsMapped { get; }
        public int Length => compiledSequence.Length;
        public int? MappingQuality { get; }

        private GenomeRead(
            bool isMapped, int? referenceId, int? readPosition,
            List<GenomeReadFeature> readFeatures, string compiledSequence, string compiledQualityScores,
            int? mappingQuality = null)
        {
            if (isMapped && (!referenceId.HasValue || !readPosition.HasValue))
            {
                throw new ArgumentException("Mapped genome reads must have a reference ID and position");
            }

            this.compiledSequence = compiledSequence;
            this.compiledQualityScores = compiledQualityScores;
            IsMapped = isMapped;
            ReferenceId = referenceId;
            ReadPosition = readPosition;
            ReadFeatures = readFeatures;
            MappingQuality = mappingQuality;
        }

        private static int CalculateLength(List<GenomeReadFeature> readFeatures)
        {
            var length = 0;
            foreach (var feature in readFeatures)
            {
                switch (feature.Type)
                {
                    case GenomeSequencePartType.Bases:
                        length += feature.Sequence.Count;
                        break;
                    case GenomeSequencePartType.QualityScores:
                        length += feature.QualityScores.Count;
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                        length += 1;
                        break;
                    case GenomeSequencePartType.Substitution:
                        length += 1;
                        break;
                    case GenomeSequencePartType.Insertion:
                        length += feature.Sequence.Count;
                        break;
                    case GenomeSequencePartType.Deletion:
                        // Deletions don't contibute to length
                        break;
                    case GenomeSequencePartType.ReferenceSkip:
                        length += feature.SkipLength.Value;
                        break;
                    case GenomeSequencePartType.SoftClip:
                        length += feature.Sequence.Count;
                        break;
                    case GenomeSequencePartType.HardClip:
                        length += feature.SkipLength.Value;
                        break;
                    case GenomeSequencePartType.Padding:
                        length += feature.SkipLength.Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return length;
        }

        public static GenomeRead MappedRead(int referenceId, int readPosition, List<GenomeReadFeature> readFeatures, int mappingQuality)
        {
            var readLength = CalculateLength(readFeatures);
            var compiledSequence = CompileSequence(readFeatures, readLength);
            var compiledQualityScores = CompileQualityScores(readFeatures, readLength);
            return new GenomeRead(
                true,
                referenceId,
                readPosition,
                readFeatures,
                compiledSequence,
                compiledQualityScores,
                mappingQuality);
        }

        public static GenomeRead UnmappedRead(IEnumerable<char> sequence, IEnumerable<char> qualityScores, int? referenceId = null, int? readPosition = null)
        {
            var compiledSequence = new string(sequence.ToArray());
            var compiledQualityScores = qualityScores != null ?new string(qualityScores.ToArray()) : null;
            var features = new List<GenomeReadFeature>
            {
                new(GenomeSequencePartType.Bases, 0, compiledSequence.ToList(), compiledQualityScores.ToList())
            };
            return new GenomeRead(
                false,
                referenceId,
                readPosition,
                features,
                compiledSequence,
                compiledQualityScores);
        }

        public string GetSequence()
        {
            return compiledSequence;
        }

        public string GetSequenceWithoutInserts()
        {
            var sequenceWithoutInserts = compiledSequence;
            foreach (var insert in ReadFeatures.Where(x => x.Type == GenomeSequencePartType.Insertion))
            {
                sequenceWithoutInserts = sequenceWithoutInserts.Remove(insert.InReadPosition, insert.Sequence.Count);
            }
            return sequenceWithoutInserts;
        }

        public string GetQualityScores()
        {
            return compiledQualityScores;
        }

        private static string CompileSequence(List<GenomeReadFeature> readFeatures, int readLength)
        {
            var sequence = new char[readLength];
            Array.Fill(sequence, '-');
            var deletionOffset = 0;
            foreach (var feature in readFeatures)
            {
                var inReadPositionWithOffset = feature.InReadPosition + deletionOffset;
                switch (feature.Type)
                {
                    case GenomeSequencePartType.Bases:
                    case GenomeSequencePartType.SoftClip:
                    case GenomeSequencePartType.Insertion:
                        Copy(feature.Sequence, 0, sequence, inReadPositionWithOffset, feature.Sequence.Count);
                        break;
                    case GenomeSequencePartType.QualityScores:
                        if(feature.Sequence != null)
                        {
                            Copy(feature.Sequence, 0, sequence, inReadPositionWithOffset, feature.Sequence.Count);
                        }
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                    case GenomeSequencePartType.Substitution:
                        sequence[inReadPositionWithOffset] = feature.Sequence[0];
                        break;
                    case GenomeSequencePartType.Deletion:
                        deletionOffset += feature.DeletionLength.Value;
                        break;
                    case GenomeSequencePartType.ReferenceSkip:
                    case GenomeSequencePartType.HardClip:
                    case GenomeSequencePartType.Padding:
                        if (feature.Sequence != null)
                        {
                            Copy(feature.Sequence, 0, sequence, inReadPositionWithOffset, feature.Sequence.Count);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return new string(sequence);
        }

        private static string CompileQualityScores(List<GenomeReadFeature> readFeatures, int readLength)
        {
            var qualityScores = new char[readLength];
            Array.Fill(qualityScores, '-');
            var deletionOffset = 0;
            foreach (var feature in readFeatures)
            {
                var inReadPositionWithOffset = feature.InReadPosition + deletionOffset;
                switch (feature.Type)
                {
                    case GenomeSequencePartType.QualityScores:
                        Copy(feature.QualityScores, 0, qualityScores, inReadPositionWithOffset, feature.QualityScores.Count);
                        break;
                    case GenomeSequencePartType.Bases:
                    case GenomeSequencePartType.SoftClip:
                    case GenomeSequencePartType.Insertion:
                        if(feature.QualityScores != null)
                        {
                            Copy(feature.QualityScores, 0, qualityScores, inReadPositionWithOffset, feature.QualityScores.Count);
                        }
                        break;
                    case GenomeSequencePartType.BaseWithQualityScore:
                        qualityScores[inReadPositionWithOffset] = feature.QualityScores[0];
                        break;
                    case GenomeSequencePartType.Substitution:
                        if(feature.QualityScores != null)
                        {
                            qualityScores[inReadPositionWithOffset] = feature.QualityScores[0];
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
                            Copy(feature.QualityScores, 0, qualityScores, inReadPositionWithOffset, feature.QualityScores.Count);
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
                target[targetIndex] = source[sourceIndex];
            }
        }

        public char GetBaseAtPosition(int inReadPosition)
        {
            if (inReadPosition >= Length)
                throw new IndexOutOfRangeException($"Position was outside of read. Position: {inReadPosition}. Length: {Length}");
            return compiledSequence[inReadPosition];
        }
    }
}