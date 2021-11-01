using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequenceAlignment
    {
        public GenomeSequenceAlignment(
            string chromosome, 
            int startIndex, 
            int endIndex,
            IGenomeSequence referenceSequence, 
            IGenomeSequence alignmentSequence, 
            List<GenomeRead> reads)
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
        public IGenomeSequence ReferenceSequence { get; }
        public IGenomeSequence AlignmentSequence { get; }
        public List<GenomeRead> Reads { get; }
    }
}