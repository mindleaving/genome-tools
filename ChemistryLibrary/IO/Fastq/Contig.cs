using ChemistryLibrary.Objects;

namespace ChemistryLibrary.IO.Fastq
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