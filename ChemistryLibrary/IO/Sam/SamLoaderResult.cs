using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class SamLoaderResult
    {
        public List<SamHeaderEntry> HeaderEntries { get; }
        public List<SamAlignmentEntry> AlignmentEntries { get; }

        public SamLoaderResult(List<SamHeaderEntry> headerEntries, List<SamAlignmentEntry> alignmentEntries)
        {
            HeaderEntries = headerEntries;
            AlignmentEntries = alignmentEntries;
        }
    }
}