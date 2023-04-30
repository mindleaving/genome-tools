using System;
using GenomeTools.ChemistryLibrary.Genomics;

namespace GenomeTools.ChemistryLibrary.IO.Bam
{
    public class BamGenomeAlignmentAccessor : IGenomeAlignmentAccessor, IDisposable
    {
        public BamGenomeAlignmentAccessor(
            )
        {

        }

        public IGenomeSequence GetReferenceSequence(
            string chromosome,
            int startIndex,
            int endIndex)
        {
            throw new NotImplementedException();
        }

        public GenomeConsensusSequence GetAlignmentSequence(
            string chromosome,
            int startIndex,
            int endIndex)
        {
            throw new NotImplementedException();
        }

        public GenomeSequenceAlignment GetAlignment(
            string chromosome,
            int startIndex,
            int endIndex)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
