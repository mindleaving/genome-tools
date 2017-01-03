using System.Collections.Generic;

namespace Domain
{
    public class Peptide
    {
        public string Chromosome { get; set; }
        public int StartBase { get; set; }
        public int EndBase { get; set; }
        public string GeneSymbol { get; set; }
        public List<char> Sequence { get; } = new List<char>();
    }
}