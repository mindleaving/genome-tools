using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            //var pdbCode1 = "4pq7";
            //var pdbCode2 = "2nxt";

            var pdbFile1 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode1}.ent";
            var pdbFile2 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode2}.ent";
            var peptide1 = PdbReader.ReadFile(pdbFile1).Models.First().Chains.First();
            var peptide2 = PdbReader.ReadFile(pdbFile2).Models.First().Chains.First();
            var proteinAligner = new ProteinAligner();
            var alignmentTransformation = proteinAligner.Align(peptide1, peptide2);
            peptide2.Molecule.Atoms
                .Where(atom => atom.IsPositioned)
                .ForEach(atom =>
                {
                    atom.IsPositionFixed = false;
                    atom.Position = alignmentTransformation.Apply(atom.Position.In(SIPrefix.Pico, Unit.Meter)).To(SIPrefix.Pico, Unit.Meter);
                });
            var repositionedPdb = PdbSerializer.Serialize(peptide2, pdbCode2);
            File.Copy(pdbFile1, $@"C:\Temp\pdb{pdbCode1}.ent", true);
            File.WriteAllText(
                Path.Combine($@"C:\Temp\pdb{pdbCode2}_repositioned_{pdbCode1}.ent"),
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

            var pdbFile1 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode1}.ent";
            var pdbFile2 = $@"G:\Projects\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb{pdbCode2}.ent";
            var peptide1 = PdbReader.ReadFile(pdbFile1).Models.First().Chains.First();
            var peptide2 = PdbReader.ReadFile(pdbFile2).Models.First().Chains.First();
            var proteinAligner = new ProteinAligner();
            var alignmentTransformation = proteinAligner.AlignSubsequence(peptide1, startIndex1, peptide2, startIndex2, length);
            peptide2.Molecule.Atoms
                .Where(atom => atom.IsPositioned)
                .ForEach(atom =>
                {
                    atom.IsPositionFixed = false;
                    atom.Position = alignmentTransformation.Apply(atom.Position.In(SIPrefix.Pico, Unit.Meter)).To(SIPrefix.Pico, Unit.Meter);
                });
            var repositionedPdb = PdbSerializer.Serialize(peptide2, pdbCode2);
            File.Copy(pdbFile1, $@"C:\Temp\pdb{pdbCode1}.ent", true);
            File.WriteAllText(
                Path.Combine($@"C:\Temp\pdb{pdbCode2}_repositioned_{pdbCode1}_sub{startIndex1}-{startIndex2}-{length}.ent"),
                repositionedPdb);
        }
    }
}
