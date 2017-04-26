using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChemistryLibrary;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Pdb;
using Commons;
using NUnit.Framework;

namespace ChemistryLibraryTest.Pdb
{
    [TestFixture]
    public class PdbReaderTest
    {
        [Test]
        public void Debug()
        {
            var filename = @"G:\Projects\HumanGenome\Protein-PDBs\5uak.pdb";
            var result = PdbReader.ReadFile(filename);
            var angleMeasurerer = new AminoAcidAngleMeasurer();
            var angleMeasurements = new Dictionary<AminoAcidReference, AminoAcidAngles>();
            var approximatePeptides = new List<ApproximatePeptide>();
            foreach (var chain in result.Chains)
            {
                var angleMeasurement = angleMeasurerer.MeasureAngles(chain);
                foreach (var kvp in angleMeasurement)
                {
                    angleMeasurements.Add(kvp.Key, kvp.Value);
                }
                var approximatePeptide = new ApproximatePeptide(chain.AminoAcids.Select(aa => aa.Name).ToList());
                for (int aminoAcidIdx = 0; aminoAcidIdx < approximatePeptide.AminoAcids.Count; aminoAcidIdx++)
                {
                    var aminoAcid = approximatePeptide.AminoAcids[aminoAcidIdx];
                    var chainReference = chain.AminoAcids[aminoAcidIdx];
                    if (angleMeasurement.ContainsKey(chainReference))
                    {
                        var angles = angleMeasurement[chainReference];
                        aminoAcid.PhiAngle = angles.Phi;
                        aminoAcid.PsiAngle = angles.Psi;
                    }
                }
                approximatePeptide.UpdatePositions();
                var approximatePeptideCompleter = new ApproximatePeptideCompleter(approximatePeptide);
                var backbone = approximatePeptideCompleter.GetBackbone();
                var peptide = approximatePeptideCompleter.GetFullPeptide();
                approximatePeptides.Add(approximatePeptide);
            }
            WriteRamachandranPlotData(@"G:\Projects\HumanGenome\ramachandranData.csv", angleMeasurements);

            //result.Chains.ForEach(chain => chain.Molecule.PositionAtoms(
            //    chain.MoleculeReference.FirstAtomId, 
            //    chain.MoleculeReference.LastAtomId));
            Assert.Pass();
        }

        private void WriteRamachandranPlotData(string filename, Dictionary<AminoAcidReference, AminoAcidAngles> measurements)
        {
            var allAngleMeasurements = measurements.Values
                .Where(m => m.Omega != null && m.Psi != null && m.Phi != null)
                .ToList();
            var lines = new List<string>();
            foreach (var measurement in allAngleMeasurements)
            {
                var line = $"{measurement.Omega.In(Unit.Degree).ToString("F2",CultureInfo.InvariantCulture)};" +
                           $"{measurement.Phi.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)};" +
                           $"{measurement.Psi.In(Unit.Degree).ToString("F2", CultureInfo.InvariantCulture)}";
                lines.Add(line);
            }
            File.WriteAllLines(filename, lines);
        }
    }
}
