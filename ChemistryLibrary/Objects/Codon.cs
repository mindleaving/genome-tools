using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class Codon
    {
        public Codon(
            AminoAcidName aminoAcid,
            List<Nucleotide> nucleotides)
        {
            AminoAcid = aminoAcid;
            Nucleotides = nucleotides;
        }

        public AminoAcidName AminoAcid { get; }
        public List<Nucleotide> Nucleotides { get; }
    }
}