using System.Linq;
using ChemistryLibrary.IO.Pdb;
using NUnit.Framework;

namespace ChemistryLibraryTest.Pdb
{
    [TestFixture]
    public class PdbReaderTest
    {
        [Test]
        [TestCase(@"G:\Projects\HumanGenome\Protein-PDBs\2mgo.pdb", 20, 9)]
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
    }
}
