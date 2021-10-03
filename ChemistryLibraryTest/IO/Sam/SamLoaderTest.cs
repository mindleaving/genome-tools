using System.IO;
using GenomeTools.ChemistryLibrary.IO.Sam;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Sam
{
    public class SamLoaderTest
    {
        const string TestDataDirectory = @"F:\datasets\mygenome\test\sam";

        private static object[] ValidSamFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "passed"), "*.sam");
        private static object[] InvalidSamFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "failed"), "*.sam");

        [Test]
        [TestCaseSource(nameof(ValidSamFiles))]
        public void CanOpenValidSamFiles(string filePath)
        {
            var sut = new SamLoader();
            SamLoaderResult result = null;
            Assert.That(() => result = sut.Load(filePath), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(InvalidSamFiles))]
        public void InvalidSamFilesThrowException(string filePath)
        {
            var sut = new SamLoader();
            Assert.That(() => sut.Load(filePath), Throws.Exception);
        }
    }
}
