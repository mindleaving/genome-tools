using GenomeTools.ChemistryLibrary.Genomics;

namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeAlignmentAccessor
    {
        IGenomeSequence GetReferenceSequence(string chromosome, int startIndex, int endIndex);
        IGenomeSequence GetAlignmentSequence(string chromosome, int startIndex, int endIndex);
        GenomeSequenceAlignment GetAlignment(string chromosome, int startIndex, int endIndex);
    }
}
