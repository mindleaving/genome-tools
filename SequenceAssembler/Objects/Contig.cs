namespace SequenceAssembler.Objects
{
    public class Contig
    {
        public Contig(byte[] nucleotides, byte[] quality)
        {
            Nucleotides = nucleotides;
            Quality = quality;
        }

        public int Length => Nucleotides.Length;
        public byte[] Nucleotides { get; }
        public byte[] Quality { get; }
    }
}