namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public class LongSequenceAlignerOptions
    {
        public int DefaultSeedLength { get; set; } = 20;
        public int MaximumSeedingTries { get; set; } = 30;
        public int ShortSequenceThreshold { get; set; } = 20;
    }
}