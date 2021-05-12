using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class Exon
    {
        public Exon(
            int startNucelotideIndex,
            List<Codon> aminoAcids)
        {
            StartNucelotideIndex = startNucelotideIndex;
            AminoAcids = aminoAcids;
        }

        public int StartNucelotideIndex { get; }
        public List<Codon> AminoAcids { get; }
        public int EndNucleotideIndex => StartNucelotideIndex + 3 * AminoAcids.Count - 1;
    }
}