using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public class RamachandranPlotFixedDistribution : IRamachandranPlotGradientDistribution
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
            var phiDiff = phi - targetPhi;
            var phiComponent = phiDiff.In(Unit.Degree) > 180 ? targetPhi + 360.To(Unit.Degree) - phi
                : phiDiff.In(Unit.Degree) < -180 ? targetPhi - phi - 360.To(Unit.Degree)
                : -phiDiff;
            var psiDiff = psi - targetPsi;
            var psiComponent = psiDiff.In(Unit.Degree) > 180 ? targetPsi + 360.To(Unit.Degree) - psi
                : psiDiff.In(Unit.Degree) < -180 ? targetPsi - psi - 360.To(Unit.Degree)
                : -psiDiff;
            return new UnitVector2D(phiComponent, psiComponent);
        }
    }
}
