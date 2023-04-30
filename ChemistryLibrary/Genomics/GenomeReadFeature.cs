using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenomeReadFeature
    {
        public GenomeReadFeature(
            GenomeSequencePartType type, 
            int inReadPosition,
            IList<char> sequence = null, 
            IList<char> qualityScores = null,
            int? deletionLength = null,
            int? skipLength = null)
        {
            Type = type;
            InReadPosition = inReadPosition;
            if (deletionLength.HasValue)
            {
                if (sequence != null || qualityScores != null || skipLength.HasValue)
                    throw new ArgumentException("If deletion length is specified no sequence, quality scores nor skip length must be specified");
                DeletionLength = deletionLength;
            }
            else if (skipLength.HasValue)
            {
                SkipLength = skipLength;
            }
            Sequence = sequence;
            QualityScores = qualityScores;
        }

        public GenomeSequencePartType Type { get; }
        public int InReadPosition { get; }
        public IList<char> Sequence { get; }
        public IList<char> QualityScores { get; }
        public int? DeletionLength { get; }
        public int? SkipLength { get; }

        public override string ToString()
        {
            switch (Type)
            {
                case GenomeSequencePartType.Bases:
                case GenomeSequencePartType.BaseWithQualityScore:
                    return new string(Sequence.ToArray());
                case GenomeSequencePartType.QualityScores:
                case GenomeSequencePartType.Substitution:
                    return Type.ToString();
                case GenomeSequencePartType.Insertion:
                case GenomeSequencePartType.SoftClip:
                    return $"{Type}: {new string(Sequence.ToArray())}";
                case GenomeSequencePartType.Deletion:
                    return $"{Type} ({DeletionLength})";
                case GenomeSequencePartType.ReferenceSkip:
                    return $"{Type} ({SkipLength})";
                case GenomeSequencePartType.HardClip:
                    return $"{Type}";
                case GenomeSequencePartType.Padding:
                    return $"{Type}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}