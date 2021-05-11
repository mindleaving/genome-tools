using System.Collections.Generic;

namespace ChemistryLibrary.Objects
{
    public class GeneLocationInfo
    {
        public string Chromosome { get; set; }
        public int StartBase { get; set; }
        public int EndBase { get; set; }
        public string GeneSymbol { get; set; }
        public List<AminoAcidName> AminoAcidSequence { get; set; } = new List<AminoAcidName>();
        public List<ExonInfo> Exons { get; set; }
    }

    public class ExonInfo
    {
        public int StartBase { get; set; }
        public int EndBase { get; set; }
        public int Length => EndBase - StartBase + 1;
    }
}