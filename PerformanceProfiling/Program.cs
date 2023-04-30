using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using GenomeTools.Studies;

namespace PerformanceProfiling
{
    public static class Program
    {
        public static void Main(string[] argv)
        {
            var chromosome = argv[0];
            Console.WriteLine($"Processing {chromosome}");
            var study = new ExonVariantStudy();
            study.ExtractVariantsOverlappingExonsFor1000Genomes(chromosome);
            //await study.ExtractVariantsOverlappingExonsForMyGenome();
            //await TestVcfAccessor();
        }

        private static async Task TestVcfAccessor()
        {
            const string Chromosome = "chrY";
            const string Vcf1000GenomesFilePath = @"F:\datasets\mygenome\OtherGenomes\genome-1000_%CHROMOSOME%.vcf";
            var vcfLoader = new ParallelizedVcfAccessor(null, Vcf1000GenomesFilePath.Replace("%CHROMOSOME%", Chromosome));
            var stopwatch = Stopwatch.StartNew();

            var variantCount = 0;

            void VariantAction(
                VcfVariantEntry variant,
                List<VcfMetadataEntry> metadata)
            {
                variantCount++;
                //Task.Delay(53).Wait();
            }

            await vcfLoader.Load(VariantAction);
            Console.WriteLine($"Variants: {variantCount}");
            Console.WriteLine($"Elapsed: {stopwatch.Elapsed}");
        }
    }
}
