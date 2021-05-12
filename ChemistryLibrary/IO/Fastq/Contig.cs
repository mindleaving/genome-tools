using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO.Fastq
{
    public class Contig
    {
        public Contig(Nucleotide[] nucleotides, byte[] quality)
        {
            Nucleotides = nucleotides;
            Quality = quality;
        }

        public int Length => Nucleotides.Length;
        public Nucleotide[] Nucleotides { get; }
        public byte[] Quality { get; }
    }
}