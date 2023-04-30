namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public interface IAlignmentRegion
    {
        AlignmentRegionType Type { get; }
        int ReferenceStartIndex { get; }
    }
}