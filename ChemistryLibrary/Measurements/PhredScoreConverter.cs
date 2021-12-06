namespace GenomeTools.ChemistryLibrary.Measurements
{
    public class PhredScoreConverter
    {
        public static readonly PhredScoreConverter Default = new();

        private readonly char lowestScore;

        public PhredScoreConverter(char lowestScore = '!')
        {
            this.lowestScore = lowestScore;
        }

        public double ToLogErrorProbability(char phredScore)
        {
            return -(phredScore - lowestScore) / 10d;
        }
    }
}
