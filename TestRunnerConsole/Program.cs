using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Vcf;

namespace TestRunnerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var vcfFilePath = @"F:\datasets\mygenome\OtherGenomes\genome-1000.vcf";
            var vcfLoader = new VcfLoader();

            var lineCount = 0;
            void AnalyzeVariant(VcfVariantEntry variantEntry, List<VcfMetadataEntry> metadata)
            {
                lineCount++;
            }

            vcfLoader.Load(vcfFilePath, AnalyzeVariant);
        }
    }
}
