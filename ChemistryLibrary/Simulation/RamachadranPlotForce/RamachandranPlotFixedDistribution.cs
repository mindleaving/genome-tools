using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public class RamachandranPlotFixedDistribution : IRamachandranPlotDistribution
    {
        private readonly UnitValue targetPhi;
        private readonly UnitValue targetPsi;

        public RamachandranPlotFixedDistribution(AminoAcidName aminoAcidName, UnitPoint2D targetPhiPsi)
        {
            targetPhi = targetPhiPsi.In(Unit.Degree).X.To(Unit.Degree);
            targetPsi = targetPhiPsi.In(Unit.Degree).Y.To(Unit.Degree);
            AminoAcidName = aminoAcidName;
        }

        public AminoAcidName AminoAcidName { get; }
        public UnitVector2D GetPhiPsiVector(UnitValue phi, UnitValue psi)
        {
            // TODO: Handle wrap-around
            return new UnitVector2D(phi - targetPhi, psi - targetPsi);
        }
    }
}
