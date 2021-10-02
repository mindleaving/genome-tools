using System.Linq;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.IO.Pdb;
using GenomeTools.ChemistryLibrary.Objects;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Pdb
{
    [TestFixture]
    public class AminoAcidAtomNamerTest
    {
        [Test]
        public void GlycineNamedCorrectly()
        {
            var aminoAcidReference = AminoAcidLibrary.Glycine(1);
            PdbAminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = moleculeStructure.GetVertexFromId(aminoAcidReference.FirstAtomId).Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = moleculeStructure.GetVertexFromId(aminoAcidReference.LastAtomId).Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => v.Object)
                .Where(atom => atom.Element != ElementName.Hydrogen)
                .Except(new[] {nitrogen, carbonEnd})
                .ToList();
            Assert.That(remainingNonHydrogenAtoms.Count, Is.EqualTo(2));
            var oxygen = remainingNonHydrogenAtoms.Single(atom => atom.Element == ElementName.Oxygen);
            Assert.That(oxygen.AminoAcidAtomName, Is.EqualTo("O"));
            var sideChainCarbon = remainingNonHydrogenAtoms.Single(atom => atom.Element == ElementName.Carbon);
            Assert.That(sideChainCarbon.AminoAcidAtomName, Is.EqualTo("CA"));
        }
        [Test]
        public void AlanineNamedCorrectly()
        {
            var aminoAcidReference = AminoAcidLibrary.Alanine(1);
            PdbAminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = moleculeStructure.GetVertexFromId(aminoAcidReference.FirstAtomId).Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = moleculeStructure.GetVertexFromId(aminoAcidReference.LastAtomId).Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => v.Object)
                .Where(atom => atom.Element != ElementName.Hydrogen)
                .Except(new[] { nitrogen, carbonEnd })
                .ToList();
            Assert.That(remainingNonHydrogenAtoms.Count, Is.EqualTo(3));
            var oxygen = remainingNonHydrogenAtoms.Single(atom => atom.Element == ElementName.Oxygen);
            Assert.That(oxygen.AminoAcidAtomName, Is.EqualTo("O"));

            var sideChainCarbons = remainingNonHydrogenAtoms.Where(atom => atom.Element == ElementName.Carbon).ToList();
            Assert.That(sideChainCarbons.Count, Is.EqualTo(2));
            Assert.That(sideChainCarbons.Select(atom => atom.AminoAcidAtomName), Is.EquivalentTo(new [] { "CA", "CB" }));
        }

        [Test]
        public void ProlineNamedCorrectly()
        {
            var aminoAcidReference = AminoAcidLibrary.Proline(1);
            PdbAminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = moleculeStructure.GetVertexFromId(aminoAcidReference.FirstAtomId).Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = moleculeStructure.GetVertexFromId(aminoAcidReference.LastAtomId).Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => v.Object)
                .Where(atom => atom.Element != ElementName.Hydrogen)
                .Except(new[] { nitrogen, carbonEnd })
                .ToList();
            Assert.That(remainingNonHydrogenAtoms.Count, Is.EqualTo(5));
            var oxygen = remainingNonHydrogenAtoms.Single(atom => atom.Element == ElementName.Oxygen);
            Assert.That(oxygen.AminoAcidAtomName, Is.EqualTo("O"));

            var sideChainCarbons = remainingNonHydrogenAtoms.Where(atom => atom.Element == ElementName.Carbon).ToList();
            Assert.That(sideChainCarbons.Count, Is.EqualTo(4));
            Assert.That(sideChainCarbons.Select(atom => atom.AminoAcidAtomName), Is.EquivalentTo(new[] { "CA", "CB", "CG", "CD" }));
        }

        [Test]
        public void LeucineNamedCorrectly()
        {
            var aminoAcidReference = AminoAcidLibrary.Leucine(1);
            PdbAminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = moleculeStructure.GetVertexFromId(aminoAcidReference.FirstAtomId).Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = moleculeStructure.GetVertexFromId(aminoAcidReference.LastAtomId).Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => v.Object)
                .Where(atom => atom.Element != ElementName.Hydrogen)
                .Except(new[] { nitrogen, carbonEnd })
                .ToList();
            Assert.That(remainingNonHydrogenAtoms.Count, Is.EqualTo(6));
            var oxygen = remainingNonHydrogenAtoms.Single(atom => atom.Element == ElementName.Oxygen);
            Assert.That(oxygen.AminoAcidAtomName, Is.EqualTo("O"));

            var sideChainCarbons = remainingNonHydrogenAtoms.Where(atom => atom.Element == ElementName.Carbon).ToList();
            Assert.That(sideChainCarbons.Count, Is.EqualTo(5));
            Assert.That(sideChainCarbons.Select(atom => atom.AminoAcidAtomName), Is.EquivalentTo(new[] { "CA", "CB", "CG", "CD1", "CD2" }));
        }

        [Test]
        public void PhenylalanineNamedCorrectly()
        {
            var aminoAcidReference = AminoAcidLibrary.Phenylalanine(1);
            PdbAminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = moleculeStructure.GetVertexFromId(aminoAcidReference.FirstAtomId).Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = moleculeStructure.GetVertexFromId(aminoAcidReference.LastAtomId).Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => v.Object)
                .Where(atom => atom.Element != ElementName.Hydrogen)
                .Except(new[] { nitrogen, carbonEnd })
                .ToList();
            Assert.That(remainingNonHydrogenAtoms.Count, Is.EqualTo(9));
            var oxygen = remainingNonHydrogenAtoms.Single(atom => atom.Element == ElementName.Oxygen);
            Assert.That(oxygen.AminoAcidAtomName, Is.EqualTo("O"));

            var sideChainCarbons = remainingNonHydrogenAtoms.Where(atom => atom.Element == ElementName.Carbon).ToList();
            Assert.That(sideChainCarbons.Count, Is.EqualTo(8));
            Assert.That(sideChainCarbons.Select(atom => atom.AminoAcidAtomName), Is.EquivalentTo(new[] { "CA", "CB", "CG", "CD1", "CD2", "CE1", "CE2", "CZ" }));
        }
    }
}
