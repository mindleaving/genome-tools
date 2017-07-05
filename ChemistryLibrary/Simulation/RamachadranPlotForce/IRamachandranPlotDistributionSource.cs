using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public interface IRamachandranPlotDistributionSource
    {
        IRamachandranPlotDistribution GetDistribution(AminoAcidName aminoAcidName);
    }
}
