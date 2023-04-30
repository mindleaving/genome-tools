using System;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class FileLevelMetadataSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.FileLevelMetadata;
        public Version FormatVersion { get; }
        public string AlignmentSortingOrder { get; }
        public string GroupingOfAlignment { get; }
        public string SubsortingOrderOfAlignments { get; }
        
        public FileLevelMetadataSamHeaderEntry(
            Version formatVersion, string alignmentSortingOrder, string groupingOfAlignment,
            string subsortingOrderOfAlignments)
        {
            FormatVersion = formatVersion;
            AlignmentSortingOrder = alignmentSortingOrder;
            GroupingOfAlignment = groupingOfAlignment;
            SubsortingOrderOfAlignments = subsortingOrderOfAlignments;
        }
    }
}