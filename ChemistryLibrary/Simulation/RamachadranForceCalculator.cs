using System;
using System.Collections.Generic;
using System.IO;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Simulation
{
    public class RamachadranForceCalculator
    {
        private readonly Dictionary<AminoAcidName, RamachandranPlotDistribution> ramachadranPlotDistributions;

        public RamachadranForceCalculator(string ramachadranDataDirectory)
        {
            var aminoAcidNames = (AminoAcidName[]) Enum.GetValues(typeof(AminoAcidName));
            foreach (var aminoAcidName in aminoAcidNames)
            {
                var distributionFilePath = Path.Combine(ramachadranDataDirectory, aminoAcidName.ToThreeLetterCode() + ".csv");
                var distribution = new RamachandranPlotDistribution(distributionFilePath);
            }
        }

        /// <summary>
        /// Use Ramachmadran plot position of an amino acid for pushing it 
        /// to keep it away from prohibited zones. 
        /// </summary>
        public Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces> CalculateForce(ApproximatePeptide peptide)
        {
            throw new NotImplementedException();
        }
    }
}