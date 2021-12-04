using System;

namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public class MismatchAlignmentRegion : IAlignmentRegion
    {
        public MismatchAlignmentRegion(
            int referenceStartIndex, 
            int referenceEndIndex, 
            int alignedSequenceStartIndex,
            int alignedSequenceEndIndex)
        {
            if (referenceStartIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(referenceStartIndex), "Reference start index must be non-negative");
            if (referenceEndIndex < referenceStartIndex)
                throw new ArgumentException("Reference end index must not be smaller than start index");
            if (alignedSequenceEndIndex < alignedSequenceStartIndex)
                throw new ArgumentException("Alignment sequence end index must not be smaller than start index");
            ReferenceStartIndex = referenceStartIndex;
            ReferenceEndIndex = referenceEndIndex;
            AlignedSequenceStartIndex = alignedSequenceStartIndex;
            AlignedSequenceEndIndex = alignedSequenceEndIndex;
        }

        public AlignmentRegionType Type => AlignmentRegionType.Mismatch;
        public int ReferenceStartIndex { get; }
        public int ReferenceEndIndex { get; }
        public int AlignedSequenceStartIndex { get; }
        public int AlignedSequenceEndIndex { get; }

        public override string ToString()
        {
            return $"Mismatch | Reference: {ReferenceStartIndex}-{ReferenceEndIndex}";
        }
    }
}