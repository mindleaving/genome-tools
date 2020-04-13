using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public interface IRamachandranPlotGradientDistributionSource
    {
        IRamachandranPlotGradientDistribution GetDistribution(AminoAcidName aminoAcidName);
    }
}
