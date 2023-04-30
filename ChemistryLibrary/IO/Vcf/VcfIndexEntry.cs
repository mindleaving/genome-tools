namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfIndexEntry
    {
        public VcfIndexEntry(string chromosome, int position, long fileOffset)
        {
            Chromosome = chromosome;
            Position = position;
            FileOffset = fileOffset;
        }

        public string Chromosome { get; }
        public int Position { get; }
        public long FileOffset { get; }
    }
}
