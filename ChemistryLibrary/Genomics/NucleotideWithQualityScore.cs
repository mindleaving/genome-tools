namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class NucleotideWithQualityScore
    {
        public char Nucleotide { get; }
        public char QualityScore { get; }

        public NucleotideWithQualityScore(char nucleotide, char qualityScore)
        {
            Nucleotide = nucleotide;
            QualityScore = qualityScore;
        }
    }
}