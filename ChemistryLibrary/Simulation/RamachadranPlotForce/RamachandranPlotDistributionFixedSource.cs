using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public class RamachandranPlotDistributionFixedSource : IRamachandranPlotDistributionSource
    {
        private readonly IRamachandranPlotDistribution distribution;

        public RamachandranPlotDistributionFixedSource(IRamachandranPlotDistribution distribution)
        {
            this.distribution = distribution;
        }

        public IRamachandranPlotDistribution GetDistribution(AminoAcidName aminoAcidName)
        {
            return distribution;
        }
    }
}