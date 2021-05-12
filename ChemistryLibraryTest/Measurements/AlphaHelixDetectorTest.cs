using System.Linq;
using GenomeTools.ChemistryLibrary.IO.Pdb;
using GenomeTools.ChemistryLibrary.Measurements;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Measurements
{
    [TestFixture]
    public class AlphaHelixDetectorTest
    {
        [Test]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\2dc3.pdb")]
        public void DetectedAlphaHelixMatchesOriginalAnnotation(string pdbFilePath)
        {
            var pdb = PdbReader.ReadFile(pdbFilePath);
            var sut = new AlphaHelixDetector();
            var peptide = pdb.Models.First().Chains.First();
            var actual = sut.Detect(peptide);
            var expectedHelixAminoAcids = peptide.Annotations
                .SelectMany(x => x.AminoAcidReferences)
                .Select(x => x.SequenceNumber)
                .ToList();
            var actualHelixAminoAcids = actual
                .SelectMany(x => x.AminoAcidReferences)
                .Select(x => x.SequenceNumber)
                .ToList();
            var truePositives = actualHelixAminoAcids.Intersect(expectedHelixAminoAcids).ToList();
            Assert.That(
                truePositives.Count, 
                Is.GreaterThan(0.9*expectedHelixAminoAcids.Count));
            var falsePositives = actualHelixAminoAcids.Except(expectedHelixAminoAcids).ToList();
            Assert.That(
                falsePositives.Count, 
                Is.LessThan(0.1*expectedHelixAminoAcids.Count));
            var falseNegatives = expectedHelixAminoAcids.Except(actualHelixAminoAcids).ToList();
            Assert.That(
                falseNegatives.Count, 
                Is.LessThan(0.1*expectedHelixAminoAcids.Count));
        }
    }
}
