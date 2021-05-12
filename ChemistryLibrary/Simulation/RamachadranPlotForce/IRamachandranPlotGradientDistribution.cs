using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public interface IRamachandranPlotGradientDistribution
    {
        AminoAcidName AminoAcidName { get; }
        UnitVector2D GetPhiPsiVector(UnitValue phi, UnitValue psi);
    }
}
