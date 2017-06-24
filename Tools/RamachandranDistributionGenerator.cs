using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons;
using NUnit.Framework;

namespace Tools
{
    [TestFixture]
    public class RamachandranDistributionGenerator
    {
        [Test]
        [TestCase(@"G:\Projects\HumanGenome\Protein-PDBs",
            @"G:\Projects\HumanGenome\ramachandranPlots",
            @"G:\Projects\HumanGenome\ramachadranDistributions")]
        public void GenerateAminoAcidDihedralAngleDistributions(string pdbDirectory, 
            string ramachandranPlotDirectory, 
            string ramachandranDistributionDirectory)
        {
            // Clear Ramachandran distribution directory, because we will be appending.
            Directory.GetFiles(ramachandranDistributionDirectory).ForEach(File.Delete);

            var pdbFiles = Directory.GetFiles(pdbDirectory, "*.pdb", SearchOption.TopDirectoryOnly);
            foreach (var pdbFile in pdbFiles)
            {
                var pdbId = Path.GetFileNameWithoutExtension(pdbFile);
                Dictionary<AminoAcidReference, AminoAcidAngles> angleMeasurements;
                try
                {
                    angleMeasurements = MeasureDihedralAngles(pdbFile);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    continue;
                }
                GenerateRamachandranPlot(angleMeasurements, ramachandranPlotDirectory, pdbId);
                GenerateRamachandranDistribution(angleMeasurements, ramachandranDistributionDirectory);
            }
        }

        private void GenerateRamachandranPlot(
            Dictionary<AminoAcidReference, AminoAcidAngles> angleMeasurements, 
            string outputDirectory,
            string pdbId)
        {
            var outputFilename = Path.Combine(outputDirectory, pdbId + ".csv");
            WriteRamachandranPlotData(outputFilename, angleMeasurements);
        }

        private void GenerateRamachandranDistribution(
            Dictionary<AminoAcidReference, AminoAcidAngles> angleMeasurements, 
            string outputDirectory)
        {
            var output = new Dictionary<AminoAcidName, List<string>>();
            foreach (var angleMeasurement in angleMeasurements)
            {
                var aminoAcidName = angleMeasurement.Key.Name;
                var measurement = angleMeasurement.Value;
                if(measurement.Omega == null || measurement.Phi == null || measurement.Psi == null)
                    continue;
                var line = $"{measurement.Omega.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)};" +
                           $"{measurement.Phi.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)};" +
                           $"{measurement.Psi.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)}";
                if(!output.ContainsKey(aminoAcidName))
                    output.Add(aminoAcidName, new List<string>());
                output[aminoAcidName].Add(line);
            }
            foreach (var aminoAcidAngles in output)
            {
                var aminoAcidName = aminoAcidAngles.Key;
                var aminoAcidCode = aminoAcidName.ToThreeLetterCode();
                var outputFilename = Path.Combine(outputDirectory, aminoAcidCode + ".csv");
                File.AppendAllLines(outputFilename, aminoAcidAngles.Value);
            }
        }

        private static Dictionary<AminoAcidReference, AminoAcidAngles> MeasureDihedralAngles(string pdbFilename)
        {
            var result = PdbReader.ReadFile(pdbFilename);
            var angleMeasurements = new Dictionary<AminoAcidReference, AminoAcidAngles>();
            foreach (var chain in result.Chains)
            {
                var angleMeasurement = AminoAcidAngleMeasurer.MeasureAngles(chain);
                foreach (var kvp in angleMeasurement)
                {
                    angleMeasurements.Add(kvp.Key, kvp.Value);
                }
            }

            return angleMeasurements;
        }

        private void WriteRamachandranPlotData(string filename,
            Dictionary<AminoAcidReference, AminoAcidAngles> measurements)
        {
            var allAngleMeasurements = measurements.Values
                .Where(m => m.Omega != null && m.Psi != null && m.Phi != null)
                .ToList();
            var lines = new List<string>();
            foreach (var measurement in allAngleMeasurements)
            {
                var line = $"{measurement.Omega.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)};" +
                           $"{measurement.Phi.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)};" +
                           $"{measurement.Psi.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)}";
                lines.Add(line);
            }
            File.WriteAllLines(filename, lines);
        }
    }
}
