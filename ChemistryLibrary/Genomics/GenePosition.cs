using Commons.Mathematics;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenePosition
    {
        public string GeneSymbol { get; }
        public string Chromosome { get; }
        public Range<int> Position { get; }

        public GenePosition(string geneSymbol, string chromosome, Range<int> position)
        {
            GeneSymbol = geneSymbol;
            Chromosome = chromosome;
            Position = position;
        }
    }
}