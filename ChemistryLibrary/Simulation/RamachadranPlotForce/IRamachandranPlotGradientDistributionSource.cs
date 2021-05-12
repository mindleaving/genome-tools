using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public interface IRamachandranPlotGradientDistributionSource
    {
        IRamachandranPlotGradientDistribution GetDistribution(AminoAcidName aminoAcidName);
    }
}
