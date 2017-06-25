using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    internal class RamachandranPlotDistribution
    {
        private readonly int gridSteps;

        public RamachandranPlotDistribution(AminoAcidName aminoAcidName, string distributionFilePath, int gridSteps = 360)
        {
            AminoAcidName = aminoAcidName;
            this.gridSteps = gridSteps;
            var angles = ParseDistributionFile(distributionFilePath);
            DistributionPlot = GenerateDistributionPlot(angles);
            GradientPlot = GenerateGradientPlot(DistributionPlot);
        }

        public AminoAcidName AminoAcidName { get; }
        public double[,] DistributionPlot { get; }
        public Vector2D[,] GradientPlot { get; }

        private double[,] GenerateDistributionPlot(List<AminoAcidAngles> aminoAcidAngles)
        {
            var distribution = new double[gridSteps, gridSteps];
            var probabilityDensitySum = 0.0;
            for (int psiStepIdx = 0; psiStepIdx < gridSteps; psiStepIdx++)
            {
                for (int phiStepIdx = 0; phiStepIdx < gridSteps; phiStepIdx++)
                {
                    // Non-normalized probability density
                    var angles = GetAnglesFromGridPosition(new Point2D(phiStepIdx, psiStepIdx));
                    var prohibitedZonePhiCenter = -30*Math.Sin(2 * angles.Psi.In(Unit.Radians));
                    var prohibitedZoneWidth = 60;

                    var phi = angles.Phi.In(Unit.Degree);
                    var distanceFromCenter = phi - prohibitedZonePhiCenter;
                    var distanceSquared = distanceFromCenter * distanceFromCenter;
                    var zoneWidthSquared = prohibitedZoneWidth * prohibitedZoneWidth;
                    var probabilityDensity = (1.0 - Math.Exp(-distanceSquared/zoneWidthSquared));

                    distribution[psiStepIdx, phiStepIdx] = probabilityDensity;
                    probabilityDensitySum += probabilityDensity;
                }
            }
            // Normalize probability densities
            for (int phiStepIdx = 0; phiStepIdx < gridSteps; phiStepIdx++)
            {
                for (int psiStepIdx = 0; psiStepIdx < gridSteps; psiStepIdx++)
                {
                    distribution[psiStepIdx, phiStepIdx] /= probabilityDensitySum;
                }
            }
            return distribution;
        }

        private static Vector2D[,] GenerateGradientPlot(double[,] distributionPlot)
        {
            var gridSteps = distributionPlot.GetLength(0);
            var gradientPlot = new Vector2D[gridSteps, gridSteps];
            for (int psiStepIdx = 0; psiStepIdx < gridSteps; psiStepIdx++)
            {
                for (int phiStepIdx = 0; phiStepIdx < gridSteps; phiStepIdx++)
                {
                    var leftPixel = phiStepIdx == 0
                        ? distributionPlot[psiStepIdx, gridSteps - 1]
                        : distributionPlot[psiStepIdx, phiStepIdx - 1];
                    var rightPixel = phiStepIdx == gridSteps - 1
                        ? distributionPlot[psiStepIdx, 0]
                        : distributionPlot[psiStepIdx, phiStepIdx+1];
                    var upperPixel = psiStepIdx == 0
                        ? distributionPlot[gridSteps - 1, phiStepIdx]
                        : distributionPlot[psiStepIdx-1, phiStepIdx];
                    var lowerPixel = psiStepIdx == gridSteps-1
                        ? distributionPlot[0, phiStepIdx]
                        : distributionPlot[psiStepIdx + 1, phiStepIdx];
                    var gradientX = (rightPixel - leftPixel) / 2.0;
                    var gradientY = (lowerPixel - upperPixel) / 2.0;
                    gradientPlot[psiStepIdx, phiStepIdx] = new Vector2D(gradientX, gradientY);
                }
            }
            return gradientPlot;
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

        public Vector2D GetGradient(UnitValue phi, UnitValue psi)
        {
            var xIdx = (int)GetGridPositionFromAngle(phi);
            var yIdx = (int)GetGridPositionFromAngle(psi);

            return GradientPlot[xIdx, yIdx];
        }

        private AminoAcidAngles GetAnglesFromGridPosition(Point2D gridPosition)
        {
            var angles = new AminoAcidAngles
            {
                Omega = 180.To(Unit.Degree),
                Phi = (360 * (gridPosition.X + 0.5) / gridSteps - 180).To(Unit.Degree),
                Psi = (180 - 360 * (gridPosition.Y + 0.5) / gridSteps).To(Unit.Degree)
            };
            return angles;
        }
        private double GetGridPositionFromAngle(UnitValue phi)
        {
            return (phi.In(Unit.Degree) + 180) * gridSteps / 360;
        }
    }
}