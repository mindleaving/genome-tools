using System.Collections.Generic;
using Commons;
using GenomeTools.ChemistryLibrary.Measurements;

namespace GenomeTools.ChemistryLibrary.DataLookups
{
    public class RamachandranPlot
    {
        public List<AminoAcidAngles> PhiPsiObservations { get; }

        public RamachandranPlot(List<AminoAcidAngles> aminoAcidAngles)
        {
            PhiPsiObservations = aminoAcidAngles;
        }

        public AminoAcidAngles GetRandomPhiPsi()
        {
            return PhiPsiObservations[StaticRandom.Rng.Next(PhiPsiObservations.Count)];
        }
    }
}
