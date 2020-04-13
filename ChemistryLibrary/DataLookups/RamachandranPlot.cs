using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Measurements;
using Commons;
using Commons.Mathematics;
using Commons.Physics;

namespace ChemistryLibrary.DataLookups
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
