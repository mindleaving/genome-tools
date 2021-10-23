using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequenceAlignment
    {
        public GenomeSequenceAlignment(string chromosome, int startIndex, int endIndex,
            string referenceSequence, string alignmentSequence, List<GenomeRead> reads)
        {
            Chromosome = chromosome;
            StartIndex = startIndex;
            EndIndex = endIndex;
            ReferenceSequence = referenceSequence;
            AlignmentSequence = alignmentSequence;
            Reads = reads;
        }

        public string Chromosome { get; }
        public int StartIndex { get; }
        public int EndIndex { get; }
        public string ReferenceSequence { get; }
        public string AlignmentSequence { get; }
        public List<GenomeRead> Reads { get; }
    }
}