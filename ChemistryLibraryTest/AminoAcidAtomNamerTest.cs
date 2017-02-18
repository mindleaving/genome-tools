using System.Linq;
using ChemistryLibrary;
using NUnit.Framework;

namespace ChemistryLibraryTest
{
    [TestFixture]
    public class AminoAcidAtomNamerTest
    {
        [Test]
        public void GlycineNamedCorrectly()
        {
            var aminoAcidReference = AminoAcidLibrary.Glycine;
            AminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = (Atom) moleculeStructure.Vertices[aminoAcidReference.FirstAtomId].Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = (Atom)moleculeStructure.Vertices[aminoAcidReference.LastAtomId].Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => (Atom) v.Value.Object)
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
            var aminoAcidReference = AminoAcidLibrary.Alanine;
            AminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = (Atom)moleculeStructure.Vertices[aminoAcidReference.FirstAtomId].Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = (Atom)moleculeStructure.Vertices[aminoAcidReference.LastAtomId].Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => (Atom)v.Value.Object)
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
            var aminoAcidReference = AminoAcidLibrary.Proline;
            AminoAcidAtomNamer.AssignNames(aminoAcidReference);

            var moleculeStructure = aminoAcidReference.Molecule.MoleculeStructure;
            var nitrogen = (Atom)moleculeStructure.Vertices[aminoAcidReference.FirstAtomId].Object;
            Assume.That(nitrogen.Element, Is.EqualTo(ElementName.Nitrogen));
            Assert.That(nitrogen.AminoAcidAtomName, Is.EqualTo("N"));

            var carbonEnd = (Atom)moleculeStructure.Vertices[aminoAcidReference.LastAtomId].Object;
            Assume.That(carbonEnd.Element, Is.EqualTo(ElementName.Carbon));
            Assert.That(carbonEnd.AminoAcidAtomName, Is.EqualTo("C"));

            var remainingNonHydrogenAtoms = moleculeStructure.Vertices
                .Select(v => (Atom)v.Value.Object)
                .Where(atom => atom.Element != ElementName.Hydrogen)
                .Except(new[] { nitrogen, carbonEnd })
                .ToList();
            Assert.That(remainingNonHydrogenAtoms.Count, Is.EqualTo(5));
            var oxygen = remainingNonHydrogenAtoms.Single(atom => atom.Element == ElementName.Oxygen);
            Assert.That(oxygen.AminoAcidAtomName, Is.EqualTo("O"));

            var sideChainCarbons = remainingNonHydrogenAtoms.Where(atom => atom.Element == ElementName.Carbon).ToList();
            Assert.That(sideChainCarbons.Count, Is.EqualTo(4));
            Assert.That(sideChainCarbons.Select(atom => atom.AminoAcidAtomName), Is.EquivalentTo(new[] { "CA", "CB", "CG", "CZ" }));
        }
    }
}
