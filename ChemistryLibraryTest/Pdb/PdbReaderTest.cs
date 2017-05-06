using System;
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

        [Test]
        public void AlphaHelixOuput()
        {
            var pdbFiles = Directory.GetFiles(@"G:\Projects\HumanGenome\Protein-PDBs", "*.pdb");
            var output = new List<string>();
            foreach (var pdbFile in pdbFiles)
            {
                try
                {
                    var pdbResult = PdbReader.ReadFile(pdbFile);
                    if(!pdbResult.Chains.Any())
                        continue;
                    output.Add("#" + Path.GetFileNameWithoutExtension(pdbFile));
                    foreach (var chain in pdbResult.Chains)
                    {
                        var helixAnnotations = chain.Annotations
                            .Where(annot => annot.Type == PeptideSecondaryStructure.AlphaHelix)
                            .ToList();
                        var fullSequence = GetFullSequence(chain, helixAnnotations);
                        //var helixSequence = GetHelixSequences(helixAnnotations);
                        output.Add(fullSequence);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
            }
            File.WriteAllLines(@"G:\Projects\HumanGenome\fullPdbSequencesHelixMarked.txt", output);
        }

        private static string GetFullSequence(Peptide chain, List<PeptideAnnotation> helixAnnotations)
        {
            var fullSequence = "";
            foreach (var aminoAcid in chain.AminoAcids)
            {
                if (helixAnnotations.Any(annotation => annotation.AminoAcidReferences.First() == aminoAcid))
                    fullSequence += "[";
                fullSequence += aminoAcid.Name.ToOneLetterCode();
                if (helixAnnotations.Any(annotation => annotation.AminoAcidReferences.Last() == aminoAcid))
                    fullSequence += "]";
            }
            return fullSequence;
        }

        private IEnumerable<string> GetHelixSequences(List<PeptideAnnotation> helixAnnotations)
        {
            foreach (var annotation in helixAnnotations)
            {
                var sequence = annotation.AminoAcidReferences.Select(aa => aa.Name.ToOneLetterCode()).ToArray();
                yield return new string(sequence);
            }
        }
    }
}
