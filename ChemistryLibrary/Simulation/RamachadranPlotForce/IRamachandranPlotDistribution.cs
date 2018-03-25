using ChemistryLibrary.Objects;
using Commons.Physics;

namespace ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public interface IRamachandranPlotDistribution
    {
        AminoAcidName AminoAcidName { get; }
        UnitVector2D GetPhiPsiVector(UnitValue phi, UnitValue psi);
    }
}
