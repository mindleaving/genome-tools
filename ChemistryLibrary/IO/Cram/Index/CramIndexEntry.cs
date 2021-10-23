namespace GenomeTools.ChemistryLibrary.IO.Cram.Index
{
    public class CramIndexEntry
    {
        public int ReferenceSequenceId { get; }
        public int AlignmentStart { get; }
        public int AlignmentSpan { get; }
        public long AbsoluteContainerOffset { get; }
        public int RelativeSliceHeaderOffset { get; }
        public int SliceSize { get; }

        public CramIndexEntry(
            int referenceSequenceId, int alignmentStart, int alignmentSpan,
            long absoluteContainerOffset, int relativeSliceHeaderOffset, int sliceSize)
        {
            ReferenceSequenceId = referenceSequenceId;
            AlignmentStart = alignmentStart;
            AlignmentSpan = alignmentSpan;
            AbsoluteContainerOffset = absoluteContainerOffset;
            RelativeSliceHeaderOffset = relativeSliceHeaderOffset;
            SliceSize = sliceSize;
        }
    }
}