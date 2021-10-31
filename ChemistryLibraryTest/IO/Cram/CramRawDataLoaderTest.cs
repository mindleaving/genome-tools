using System.IO;
using GenomeTools.ChemistryLibrary.IO.Cram;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class CramRawDataLoaderTest
    {
        const string TestDataDirectory = @"F:\datasets\mygenome\test\cram\3.0";
        private static object[] ValidCramFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "passed"), "*.cram");
        private static object[] InvalidCramFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "failed"), "*.cram");

        [Test]
        [TestCaseSource(nameof(ValidCramFiles))]
        public void CanOpenValidCramFiles(string filePath)
        {
            var sut = new CramRawDataLoader();
            CramRawData result = null;
            Assert.That(() => result = sut.Load(filePath), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(InvalidCramFiles))]
        public void InvalidCramFilesThrowException(string filePath)
        {
            var sut = new CramRawDataLoader();
            Assert.That(() => sut.Load(filePath), Throws.Exception);
        }
    }
}