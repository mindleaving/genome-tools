using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ChemistryLibrary.Measurements;
using Commons;

namespace ChemistryLibrary.Simulation
{
    internal class RamachandranPlotDistribution
    {
        public RamachandranPlotDistribution(string distributionFilePath)
        {
            var angles = ParseDistributionFile(distributionFilePath);
            Distribution = GenerateDistribution(angles, 360);
        }

        private double[,] GenerateDistribution(List<AminoAcidAngles> aminoAcidAngles, int gridSteps)
        {
            //var stepSize = 360.0 / gridSteps;
            var distribution = new double[gridSteps, gridSteps];
            for (int phiStepIdx = 0; phiStepIdx < gridSteps; phiStepIdx++)
            {
                var phi = 360 * (phiStepIdx + 0.5) / gridSteps - 180;
                for (int psiStepIdx = 0; psiStepIdx < gridSteps; psiStepIdx++)
                {
                    var psi = 360 * (psiStepIdx + 0.5) / gridSteps - 180;

                    var probabilityDensity = double.NaN; // TODO

                    distribution[psiStepIdx, phiStepIdx] = probabilityDensity;
                }
            }
            return distribution;
        }

        private static List<AminoAcidAngles> ParseDistributionFile(string distributionFilePath)
        {
            const char Delimiter = ';';

            var angles = new List<AminoAcidAngles>();
            using (var streamReader = new StreamReader(distributionFilePath))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if(line.StartsWith("#") || line.StartsWith("//"))
                        continue;
                    var splittedLine = line.Split(Delimiter);
                    if(splittedLine.Length != 3)
                        continue;
                    var omega = double.Parse(splittedLine[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                    var phi = double.Parse(splittedLine[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    var psi = double.Parse(splittedLine[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                    angles.Add(new AminoAcidAngles
                    {
                        Omega = omega.To(Unit.Degree),
                        Phi = phi.To(Unit.Degree),
                        Psi = psi.To(Unit.Degree)
                    });
                }
            }
            return angles;
        }

        public double[,] Distribution { get; }

        public double GetOutlierProbability(AminoAcidAngles dihedralAngles)
        {
            throw new NotImplementedException();
        }
    }
}