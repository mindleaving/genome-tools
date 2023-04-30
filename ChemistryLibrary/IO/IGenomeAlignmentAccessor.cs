using GenomeTools.ChemistryLibrary.Genomics;

namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeAlignmentAccessor
    {
        IGenomeSequence GetReferenceSequence(string chromosome, int startIndex, int endIndex);
        GenomeConsensusSequence GetAlignmentSequence(string chromosome, int startIndex, int endIndex);
        GenomeSequenceAlignment GetAlignment(string chromosome, int startIndex, int endIndex);
    }
}
