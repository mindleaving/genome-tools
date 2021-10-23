namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeAlignmentAccessor
    {
        GenomeSequence GetReferenceSequence(string chromosome, int startIndex, int endIndex);
        GenomeSequence GetAlignmentSequence(string chromosome, int startIndex, int endIndex);
        GenomeSequenceAlignment GetAlignment(string chromosome, int startIndex, int endIndex);
    }
}
