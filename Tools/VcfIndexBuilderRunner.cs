﻿using GenomeTools.ChemistryLibrary.IO.Vcf;
using NUnit.Framework;

namespace GenomeTools.Tools
{
    public class VcfIndexBuilderRunner
    {
        [Test]
        [TestCase(@"F:\datasets\mygenome\genome-janscholtyssek.vcf")]
        public void Run(string filePath)
        {
            var indexBuilder = new VcfIndexBuilder();
            indexBuilder.Build(filePath, 50);
        }
    }
}
