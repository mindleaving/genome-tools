using System.IO;
using GenomeTools.ChemistryLibrary.IO.Cram;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class CramRawDataLoaderTest
    {
        const string TestDataDirectory = @"F:\datasets\mygenome\test\cram\3.0";
        const string ReferenceSequenceFilePath = @"F:\datasets\mygenome\test\cram\ce.fa";
        private static object[] ValidCramFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "passed"), "*.cram");
        private static object[] InvalidCramFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "failed"), "*.cram");

        // NOTE: Some files fail because of failing MD5-checks.
        // These files contain a non-existing reference sequence and hence the thrown error is correctly thrown.

        [Test]
        [TestCaseSource(nameof(ValidCramFiles))]
        public void CanOpenValidCramFiles(string filePath)
        {
            var sut = new CramRawDataLoader();
            CramRawData result = null;
            Assert.That(() => result = sut.Load(filePath, ReferenceSequenceFilePath), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(InvalidCramFiles))]
        public void InvalidCramFilesThrowException(string filePath)
        {
            var sut = new CramRawDataLoader();
            Assert.That(() => sut.Load(filePath, ReferenceSequenceFilePath), Throws.Exception);
        }
    }
}