using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.IO;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation.RamachadranPlotForce
{
    public class RamachandranPlotGridGradientDistribution : IRamachandranPlotGradientDistribution
    {
        private readonly int gridSteps;

        public AminoAcidName AminoAcidName { get; }
        public Vector2D[,] GradientPlot { get; }

        public RamachandranPlotGridGradientDistribution(AminoAcidName aminoAcidName, string distributionFilePath, int gridSteps = 360)
        {
            AminoAcidName = aminoAcidName;
            this.gridSteps = gridSteps;
            var distributionCacheFilename = GetCachedFilename(distributionFilePath);
            try
            {
                GradientPlot = LoadGradientGrid(distributionCacheFilename);
            }
            catch (Exception)
            {
                var angles = RamachandranPlotFileSource.ParseRamachadranPlotFile(distributionFilePath);
                GradientPlot = GenerateGradientPlot(angles);
                StoreDistibutionPlot(distributionCacheFilename);
            }
        }

        private string GetCachedFilename(string distributionFilePath)
        {
            var directory = Path.GetDirectoryName(distributionFilePath);
            if (directory == null)
            {
                return Path.Combine("Cache", AminoAcidName.ToThreeLetterCode() + ".csv");
            }
            return Path.Combine(
                directory,
                "Cache",
                AminoAcidName.ToThreeLetterCode() + ".csv");
        }

        private Vector2D[,] LoadGradientGrid(string distributionCacheFilename)
        {
            var xGridFilename = distributionCacheFilename.Replace(".csv", "_X.csv");
            var xGrid = CsvReader.ReadDoubleArray(xGridFilename);
            if(xGrid.GetLength(0) != gridSteps || xGrid.GetLength(1) != gridSteps)
                throw new Exception("Cached grid has wrong dimension");
            var yGridFilename = distributionCacheFilename.Replace(".csv", "_Y.csv");
            var yGrid = CsvReader.ReadDoubleArray(yGridFilename);
            if (yGrid.GetLength(0) != gridSteps || yGrid.GetLength(1) != gridSteps)
                throw new Exception("Cached grid has wrong dimension");
            var gradientGrid = new Vector2D[gridSteps,gridSteps];
            for (var rowIdx = 0; rowIdx < gridSteps; rowIdx++)
            {
                for (var columnIdx = 0; columnIdx < gridSteps; columnIdx++)
                {
                    gradientGrid[rowIdx, columnIdx] = new Vector2D(xGrid[rowIdx, columnIdx], yGrid[rowIdx, columnIdx]);
                }
            }
            return gradientGrid;
        }

        private void StoreDistibutionPlot(string distributionCacheFilename)
        {
            var directory = Path.GetDirectoryName(distributionCacheFilename);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string xGridFilename;
            string yGridFilename;
            if (distributionCacheFilename.EndsWith(".csv"))
            {
                xGridFilename = distributionCacheFilename.Replace(".csv", "_X.csv");
                yGridFilename = distributionCacheFilename.Replace(".csv", "_Y.csv");
            }
            else
            {
                xGridFilename = distributionCacheFilename + "_X.csv";
                yGridFilename = distributionCacheFilename + "_Y.csv";
            }
            var xGrid = new double[gridSteps,gridSteps];
            var yGrid = new double[gridSteps, gridSteps];
            for (var rowIdx = 0; rowIdx < gridSteps; rowIdx++)
            {
                for (var columnIdx = 0; columnIdx < gridSteps; columnIdx++)
                {
                    xGrid[rowIdx, columnIdx] = GradientPlot[rowIdx, columnIdx].X;
                    yGrid[rowIdx, columnIdx] = GradientPlot[rowIdx, columnIdx].Y;
                }
            }
            CsvWriter.Write(xGrid, xGridFilename);
            CsvWriter.Write(yGrid, yGridFilename);
        }

        private Vector2D[,] GenerateGradientPlot(List<AminoAcidAngles> aminoAcidAngles)
        {
            const double Sigma = 10.0;

            var gradientPlot = new Vector2D[gridSteps, gridSteps];
            Parallel.For(0, gridSteps, psiStepIdx =>
            {
                for (int phiStepIdx = 0; phiStepIdx < gridSteps; phiStepIdx++)
                {
                    var gridAngle = GetAnglesFromGridPosition(new Point2D(phiStepIdx, psiStepIdx));
                    var gradientVector = new Vector2D(0, 0);
                    var totalWeight = 0.0;
                    foreach (var aminoAcidAngle in aminoAcidAngles)
                    {
                        var phiDiff = gridAngle.Phi - aminoAcidAngle.Phi;
                        var psiDiff = gridAngle.Psi - aminoAcidAngle.Psi;
                        var phiComponent = phiDiff.In(Unit.Degree) > 180
                            ? aminoAcidAngle.Phi + 360.To(Unit.Degree) - gridAngle.Phi
                            : phiDiff.In(Unit.Degree) < -180
                                ? aminoAcidAngle.Phi - gridAngle.Phi - 360.To(Unit.Degree)
                                : -phiDiff;
                        var psiComponent = psiDiff.In(Unit.Degree) > 180
                            ? aminoAcidAngle.Psi + 360.To(Unit.Degree) - gridAngle.Psi
                            : psiDiff.In(Unit.Degree) < -180
                                ? aminoAcidAngle.Psi - gridAngle.Psi - 360.To(Unit.Degree)
                                : -psiDiff;
                        var phiComponentDegree = phiComponent.In(Unit.Degree);
                        var psiComponentDegree = psiComponent.In(Unit.Degree);
                        var distanceToPhiPsiSquared = phiComponentDegree * phiComponentDegree
                                                      + psiComponentDegree * psiComponentDegree;
                        var distanceToPhiPsi = Math.Sqrt(distanceToPhiPsiSquared);
                        var weight = (1 - Math.Exp(-distanceToPhiPsiSquared / (2 * Sigma * Sigma))) / distanceToPhiPsi;
                        totalWeight += weight;
                        gradientVector.X += weight * phiComponentDegree;
                        gradientVector.Y += weight * psiComponentDegree;
                    }
                    gradientVector.X /= totalWeight;
                    gradientVector.Y /= totalWeight;
                    gradientPlot[psiStepIdx, phiStepIdx] = gradientVector;
                }
            });
            return gradientPlot;
        }

        public UnitVector2D GetPhiPsiVector(UnitValue phi, UnitValue psi)
        {
            var xIdx = (int)GetPhiGridPosition(phi);
            var yIdx = (int)GetPPsiGridPosition(psi);

            return GradientPlot[xIdx, yIdx].To(Unit.Degree);
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
        private double GetPhiGridPosition(UnitValue phi)
        {
            return (phi.In(Unit.Degree) + 180) * gridSteps / 360;
        }
        private double GetPPsiGridPosition(UnitValue psi)
        {
            return gridSteps * (1-(psi.In(Unit.Degree) + 180) / 360);
        }
    }
}