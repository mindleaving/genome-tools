using System;
using System.Collections.Generic;
using System.IO;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public class RamachandranPlotDistributionFileSource : IRamachandranPlotDistributionSource
    {
        private readonly Dictionary<AminoAcidName, IRamachandranPlotDistribution> distributions;

        public RamachandranPlotDistributionFileSource(string distributionDirectory)
        {
            distributions = new Dictionary<AminoAcidName, IRamachandranPlotDistribution>();
            var aminoAcidNames = (AminoAcidName[])Enum.GetValues(typeof(AminoAcidName));
            foreach (var aminoAcidName in aminoAcidNames)
            {
                var distributionFilePath = Path.Combine(distributionDirectory, aminoAcidName.ToThreeLetterCode() + ".csv");
                var distribution = new RamachandranPlotGridDistribution(aminoAcidName, distributionFilePath);
                distributions.Add(aminoAcidName, distribution);
                //if (aminoAcidName == AminoAcidName.Alanine)
                //{
                //    CsvWriter.Write(distribution.DistributionPlot, @"G:\Projects\HumanGenome\ALA_distibution.csv");
                //    return;
                //}
            }
        }

        public IRamachandranPlotDistribution GetDistribution(AminoAcidName aminoAcidName)
        {
            return distributions[aminoAcidName];
        }
    }
}