using System.IO;
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

            // We don't know the reference sequence IDs and hence cannot test for valid entries of a particular reference ID
            //Assert.That(result.GetEntriesForReferenceSequence(-1), Is.Not.Empty);
        }
    }
}
