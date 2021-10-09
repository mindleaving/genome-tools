using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Sam;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramHeader
    {
        public List<SamHeaderEntry> SamHeader { get; }

        public CramHeader(List<SamHeaderEntry> samHeader)
        {
            SamHeader = samHeader;
        }
    }
}