using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public class RamachandranPlotDistributionFixedSource : IRamachandranPlotGradientDistributionSource
    {
        private readonly IRamachandranPlotGradientDistribution distribution;

        public RamachandranPlotDistributionFixedSource(IRamachandranPlotGradientDistribution distribution)
        {
            this.distribution = distribution;
        }

        public IRamachandranPlotGradientDistribution GetDistribution(AminoAcidName aminoAcidName)
        {
            return distribution;
        }
    }
}