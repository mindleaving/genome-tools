namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public class MatchedAlignmentRegion : IAlignmentRegion
    {
        public MatchedAlignmentRegion(
            int referenceStartIndex, 
            int alignedSequenceStartIndex, 
            int alignmentLength)
        {
            ReferenceStartIndex = referenceStartIndex;
            AlignedSequenceStartIndex = alignedSequenceStartIndex;
            AlignmentLength = alignmentLength;
        }

        public AlignmentRegionType Type => AlignmentRegionType.Match;
        public int AlignmentLength { get; }
        public int ReferenceStartIndex { get; }
        public int ReferenceEndIndex => ReferenceStartIndex + AlignmentLength - 1;
        public int AlignedSequenceStartIndex { get; }
        public int AlignedSequenceEndIndex => AlignedSequenceStartIndex + AlignmentLength - 1;

        public override string ToString()
        {
            return $"Match | Reference: {ReferenceStartIndex}-{ReferenceEndIndex}";
        }
    }
}