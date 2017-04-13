using ChemistryLibrary.Pdb;
using NUnit.Framework;

namespace ChemistryLibraryTest.Pdb
{
    [TestFixture]
    public class PdbReaderTest
    {
        [Test]
        public void Debug()
        {
            var filename = @"G:\Projects\HumanGenome\Protein-PDBs\5uww.pdb";
            var result = PdbReader.ReadFile(filename);

            result.Chains.ForEach(chain => chain.Molecule.PositionAtoms(
                chain.MoleculeReference.FirstAtomId, 
                chain.MoleculeReference.LastAtomId));
            Assert.Pass();
        }
    }
}
