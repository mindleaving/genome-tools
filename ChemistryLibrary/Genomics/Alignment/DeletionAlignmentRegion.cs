namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public class DeletionAlignmentRegion : IAlignmentRegion
    {
        public DeletionAlignmentRegion(
            int referenceStartIndex,
            int deletionLength)
        {
            ReferenceStartIndex = referenceStartIndex;
            DeletionLength = deletionLength;
        }

        public AlignmentRegionType Type => AlignmentRegionType.Deletion;
        public int ReferenceStartIndex { get; }
        public int ReferenceEndIndex => ReferenceStartIndex + DeletionLength - 1;
        public int DeletionLength { get; }

        public override string ToString()
        {
            return $"Deletion | Reference: {ReferenceStartIndex}-{ReferenceEndIndex}";
        }
    }
}