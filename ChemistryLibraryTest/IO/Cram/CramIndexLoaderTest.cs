using System.IO;
using GenomeTools.ChemistryLibrary.IO.Cram;
using GenomeTools.ChemistryLibrary.IO.Cram.Index;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class CramIndexLoaderTest
    {
        const string TestDataDirectory = @"F:\datasets\mygenome\test\cram\3.0";
        private static object[] ValidCramIndexFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "passed"), "*.cram.crai");

        [Test]
        [TestCaseSource(nameof(ValidCramIndexFiles))]
        public void CanReadIndexFiles(string filePath)
        {
            var sut = new CramIndexLoader();
            CramIndex result = null;
            Assert.That(() => result = sut.Load(filePath), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.GetEntriesForReferenceSequence(-1), Is.Not.Empty);
        }
    }
}
