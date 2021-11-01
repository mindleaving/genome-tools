using System;
using System.Collections.Generic;

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
    }
}