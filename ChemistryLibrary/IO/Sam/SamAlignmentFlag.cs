using System;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    [Flags]
    public enum SamAlignmentFlag : uint
    {
        TemplateMultipleSegments = 1 << 0,
        ProperlyAligned = 1 << 1,
        Unmapped = 1 << 2,
        NextSegmentUnmapped = 1 << 3,
        SeqReverseComplement = 1 << 4,
        SeqNextSegmentReverseComplement = 1 << 5,
        FirstSegment = 1 << 6,
        LastSegment = 1 << 7,
        SecondaryAlignment = 1 << 8,
        LowQuality = 1 << 9,
        PcrOrOpticalDuplicate = 1 << 10,
        SupplementaryAlignment = 1 << 11
    }
}