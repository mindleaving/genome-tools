using System.Linq;
using ChemistryLibrary.IO.Pdb;
using NUnit.Framework;

namespace ChemistryLibraryTest.Pdb
{
    [TestFixture]
    public class PdbReaderTest
    {
        [Test]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\2mgo.pdb", 20, 9)]
        public void AllModelsRead(string file, int expectedModelCount, int expectedPeptideLength)
        {
            var pdbResult = PdbReader.ReadFile(file);
            Assert.That(pdbResult.Models.Count, Is.EqualTo(expectedModelCount));
            foreach (var pdbModel in pdbResult.Models)
            {
                Assert.That(pdbModel.Chains.Count, Is.EqualTo(1));
                var peptide = pdbModel.Chains.Single();
                Assert.That(peptide.AminoAcids.Count, Is.EqualTo(expectedPeptideLength));
            }
        }

        [Test]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\HumanProteins\SingleChain\FullyPositioned\pdb1b68.ent", 23, 162)]
        public void AminoAcidSequenceNumberAsExpected(string pdbFile, int sequenceStart, int sequenceStop)
        {
            var pdbResult = PdbReader.ReadFile(pdbFile);
            Assume.That(pdbResult.Models.Any());
            var firstModel = pdbResult.Models.First();
            var firstChain = firstModel.Chains.First();
            Assert.That(firstChain.AminoAcids.First().SequenceNumber, Is.EqualTo(sequenceStart));
            Assert.That(firstChain.AminoAcids.Last().SequenceNumber, Is.EqualTo(sequenceStop));
        }
    }
}
