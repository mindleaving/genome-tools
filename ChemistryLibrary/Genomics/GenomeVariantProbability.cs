namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenomeVariantProbability
    {
        public double HomozygousProbability { get; }
        public double HeterozygousProability { get; }

        public GenomeVariantProbability(double homozygousProbability, double heterozygousProability)
        {
            HomozygousProbability = homozygousProbability;
            HeterozygousProability = heterozygousProability;
        }
    }
}
