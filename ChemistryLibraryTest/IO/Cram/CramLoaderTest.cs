using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class CramLoaderTest
    {
        private const string filePath = "/mnt/encrypted/backup/mygenome/genome.cram";

        public void CanReadFileDefinition()
        {
            var sut = new CramLoader();
            var actual = sut.Load(filePath);
            Assert.That(actual, Is.Not.Null);
        }
    }
}