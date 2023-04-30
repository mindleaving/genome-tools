namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public class InsertAlignmentRegion : IAlignmentRegion
    {
        public InsertAlignmentRegion(
            int referenceStartIndex, 
            int alignedSequenceStartIndex, 
            int insertLength)
        {
            ReferenceStartIndex = referenceStartIndex;
            AlignedSequenceStartIndex = alignedSequenceStartIndex;
            InsertLength = insertLength;
        }

        public AlignmentRegionType Type => AlignmentRegionType.Insert;
        public int ReferenceStartIndex { get; }
        public int AlignedSequenceStartIndex { get; }
        public int InsertLength { get; }

        public override string ToString()
        {
            return $"Insertion | Reference: {ReferenceStartIndex}, +{InsertLength}";
        }
    }
}