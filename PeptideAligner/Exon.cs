using System.Collections.Generic;

namespace PeptideAligner
{
    public class Exon
    {
        public int StartNucelotideIndex { get; set; }
        public List<char> AminoAcids { get; } = new List<char>();
    }
}