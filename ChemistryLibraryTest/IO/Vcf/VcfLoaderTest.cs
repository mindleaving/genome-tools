﻿using System.IO;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Vcf
{
    public class VcfLoaderTest
    {
        private const string TestDataDirectory = @"F:\datasets\mygenome\test\vcf\4.3";

        private static object[] ValidVcfFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "passed"), "*.vcf");
        private static object[] InvalidVcfFiles = Directory.GetFiles(Path.Combine(TestDataDirectory, "failed"), "*.vcf");

        [Test]
        [TestCaseSource(nameof(ValidVcfFiles))]
        public void CanOpenValidVcfFiles(string filePath)
        {
            var sut = new ParallelizedVcfAccessor(null, filePath);
            VcfLoaderResult result = null;
            Assert.That(async () => result = await sut.Load(), Throws.Nothing);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(InvalidVcfFiles))]
        public void InvalidVcfFilesThrowException(string filePath)
        {
            var sut = new ParallelizedVcfAccessor(null, filePath);
            Assert.That(async () => await sut.Load(), Throws.Exception);
        }
    }
}
