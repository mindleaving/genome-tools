using System;
using System.Collections.Generic;
using System.IO;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public class RamachandranPlotGradientDistributionFileSource : IRamachandranPlotGradientDistributionSource
    {
        private readonly Dictionary<AminoAcidName, IRamachandranPlotGradientDistribution> distributions;

        public RamachandranPlotGradientDistributionFileSource(string distributionDirectory)
        {
            distributions = new Dictionary<AminoAcidName, IRamachandranPlotGradientDistribution>();
            var aminoAcidNames = (AminoAcidName[])Enum.GetValues(typeof(AminoAcidName));
            foreach (var aminoAcidName in aminoAcidNames)
            {
                var distributionFilePath = Path.Combine(distributionDirectory, aminoAcidName.ToThreeLetterCode() + ".csv");
                var distribution = new RamachandranPlotGridGradientDistribution(aminoAcidName, distributionFilePath);
                distributions.Add(aminoAcidName, distribution);
            }
        }

        public IRamachandranPlotGradientDistribution GetDistribution(AminoAcidName aminoAcidName)
        {
            return distributions[aminoAcidName];
        }
    }
}