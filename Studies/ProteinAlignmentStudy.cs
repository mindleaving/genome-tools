using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Physics;
using NUnit.Framework;

namespace Studies
{
    /// <summary>
    /// Overlay proteins with each other at different sequences and study the relationship between 3D similarity and amino acid sequence
    /// </summary>
    [TestFixture]
    public class ProteinAlignmentStudy
    {
        [Test]
        public void Debug()
        {
            var aligner = new ProteinAligner();
            var sequence1 = new List<AminoAcidName>
            {
                AminoAcidName.Alanine,
                AminoAcidName.Glutamine,
                AminoAcidName.Glycine,
                AminoAcidName.Isoleucine
            };
            var sequence2 = new List<AminoAcidName>
            {
                AminoAcidName.Histidine,
                AminoAcidName.Glycine,
                AminoAcidName.Isoleucine,
                AminoAcidName.Proline,
                AminoAcidName.Methionine
            };
            var alignmentResult = SequenceAligner.Align(sequence1, sequence2);
            Assert.That(alignmentResult, Is.Not.Null);
        }

        [Test]
        public void AlignPdbs()
        {
            var pdbCode1 = "1xmj";
            var pdbCode2 = "2bbo";
            var outputDirectory = @"C:\Temp";

            var pdbFile1 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode1}.ent";
            var pdbFile2 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode2}.ent";
            var peptide1 = PdbReader.ReadFile(pdbFile1).Models.First().Chains.First();
            var peptide2 = PdbReader.ReadFile(pdbFile2).Models.First().Chains.First();
            var proteinAligner = new ProteinAligner();
            var proteinAlignerResult = proteinAligner.Align(peptide1, peptide2);
            var alignmentTransform = proteinAlignerResult.Transformation;
            peptide2.Molecule.Atoms
                .Where(atom => atom.IsPositioned)
                .ForEach(atom =>
                {
                    atom.IsPositionFixed = false;
                    atom.Position = alignmentTransform.Apply(atom.Position.In(SIPrefix.Pico, Unit.Meter)).To(SIPrefix.Pico, Unit.Meter);
                });
            var repositionedPdb = PdbSerializer.Serialize(pdbCode2, peptide2);
            File.Copy(pdbFile1, Path.Combine(outputDirectory, $@"pdb{pdbCode1}.ent"), true);
            File.WriteAllText(
                Path.Combine(outputDirectory, $@"pdb{pdbCode2}_repositioned_{pdbCode1}.ent"),
                repositionedPdb);
        }

        [Test]
        public void AlignPdbSubsequences()
        {
            var pdbCode1 = "1xmj";
            var startIndex1 = 48;
            var pdbCode2 = "2bbo";
            var startIndex2 = 60;
            var length = 40;
            var outputDirectory = @"C:\Temp";

            var pdbFile1 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode1}.ent";
            var pdbFile2 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode2}.ent";
            var peptide1 = PdbReader.ReadFile(pdbFile1).Models.First().Chains.First();
            var peptide2 = PdbReader.ReadFile(pdbFile2).Models.First().Chains.First();
            var proteinAligner = new ProteinAligner();
            var proteinAlignerResult = proteinAligner.AlignSubsequence(peptide1, startIndex1, peptide2, startIndex2, length);
            var alignmentTransform = proteinAlignerResult.Transformation;
            peptide2.Molecule.Atoms
                .Where(atom => atom.IsPositioned)
                .ForEach(atom =>
                {
                    atom.IsPositionFixed = false;
                    atom.Position = alignmentTransform.Apply(atom.Position.In(SIPrefix.Pico, Unit.Meter)).To(SIPrefix.Pico, Unit.Meter);
                });
            var repositionedPdb = PdbSerializer.Serialize(pdbCode2, peptide2);
            File.Copy(pdbFile1, Path.Combine(outputDirectory, $@"pdb{pdbCode1}.ent"), true);
            File.WriteAllText(
                Path.Combine(outputDirectory, $@"pdb{pdbCode2}_repositioned_{pdbCode1}_sub{startIndex1}-{startIndex2}-{length}.ent"),
                repositionedPdb);
        }

        [Test]
        public void AlignAllModelsOfProtein(string proteinName, bool storeIndividualAlignedPdb)
        {
            var outputDirectory = Path.Combine(@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\AlignedProteins", proteinName);
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            var proteinListDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\ByProtein";
            var pdbListFile = Path.Combine(proteinListDirectory, proteinName + ".csv");
            var pdbFiles = File.ReadLines(pdbListFile).ToList();
            var firstPeptide = PdbReader.ReadFile(pdbFiles.First()).Models.First().Chains.First();
            File.Copy(pdbFiles.First(), Path.Combine(outputDirectory, Path.GetFileName(pdbFiles.First())), true);
            var proteinAligner = new ProteinAligner();
            var combinedModels = new List<Peptide> {firstPeptide};
            var modelErrors = new Dictionary<string, UnitValue> {{pdbFiles.First(), 0.To(Unit.Meter)}};
            foreach (var pdbFile in pdbFiles.Skip(1))
            {
                var peptide = PdbReader.ReadFile(pdbFile).Models.First().Chains.First();
                var proteinAlignerResult = proteinAligner.Align(firstPeptide, peptide);
                var alignmentTransform = proteinAlignerResult.Transformation;
                peptide.Molecule.Atoms
                    .Where(atom => atom.IsPositioned)
                    .ForEach(atom =>
                    {
                        atom.IsPositionFixed = false;
                        atom.Position = alignmentTransform.Apply(atom.Position.In(SIPrefix.Pico, Unit.Meter)).To(SIPrefix.Pico, Unit.Meter);
                    });
                var modelError = proteinAlignerResult.IsTransformationValid
                    ? proteinAlignerResult.AveragePositionError
                    : double.PositiveInfinity.To(Unit.Meter);
                modelErrors.Add(pdbFile, modelError);
                combinedModels.Add(peptide);
                if(storeIndividualAlignedPdb)
                {
                    var pdbId = Path.GetFileNameWithoutExtension(pdbFile).Replace("pdb", "");
                    var repositionedPdb = PdbSerializer.Serialize(pdbId, peptide);
                    File.WriteAllText(
                        Path.Combine(outputDirectory, $"pdb{pdbId}.ent"),
                        repositionedPdb);
                }
            }

            var medianError = modelErrors.Values.Select(x => x.In(SIPrefix.Pico, Unit.Meter)).Median();
            var stdError = modelErrors.Values
                .Select(x => x.In(SIPrefix.Pico, Unit.Meter))
                .Average(x => x.Square()).Sqrt();
            var validModels = pdbFiles
                .Select((pdbFile, idx) => new
                {
                    PdbFile = pdbFile,
                    Model = combinedModels[idx],
                    Error = modelErrors[pdbFile].In(SIPrefix.Pico, Unit.Meter)
                })
                .Where(x => x.Error < Math.Min(medianError + 2 * stdError, 1000))
                .Select(x => x.Model)
                .ToArray();

            var combinedPdb = PdbSerializer.Serialize("1234", validModels);
            File.WriteAllText(
                Path.Combine(outputDirectory, "pdb_combined.ent"),
                combinedPdb);
            File.WriteAllLines(
                Path.Combine(outputDirectory, "averageError.csv"),
                modelErrors.Select(kvp => $"{kvp.Key};{kvp.Value.In(SIPrefix.Pico, Unit.Meter)}"));
        }

        [Test]
        public void AlignAllProteins()
        {
            var proteinFileDirectory = @"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\ByProtein";
            var storeIndividualAlignedPdb = false;

            var proteinFiles = Directory.EnumerateFiles(proteinFileDirectory);
            foreach (var proteinFile in proteinFiles)
            {
                var proteinName = Path.GetFileNameWithoutExtension(proteinFile);
                var lineCount = File.ReadLines(proteinFile).Count();
                if (lineCount < 10)
                    continue;
                try
                {
                    AlignAllModelsOfProtein(proteinName, storeIndividualAlignedPdb);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Alignment failed for {proteinName}: {e.Message}");
                }
            }
        }
    }
}
