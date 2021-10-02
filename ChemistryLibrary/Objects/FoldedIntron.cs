using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class FoldedIntron
    {
        public Intron Intron { get; }
        public List<BasePairing> BasePairings { get; }

        public FoldedIntron(
            Intron intron,
            List<BasePairing> basePairings)
        {
            Intron = intron;
            BasePairings = basePairings;
        }
    }
    public class BasePairing
    {
        public BasePairing(
            int base1Index,
            int base2Index)
        {
            Base1Index = base1Index;
            Base2Index = base2Index;
        }

        public int Base1Index { get; }
        public int Base2Index { get; }
    }
}