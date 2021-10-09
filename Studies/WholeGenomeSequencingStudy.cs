using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    public class WholeGenomeSequencingStudy
    {
        [Test]
        [TestCase(@"F:\datasets\mygenome\genome-janscholtyssek.vcf")]
        public void AnalyzeVariantPositionDistribution(string vcfFilePath)
        {
            var vcfLoader = new VcfLoader();
            var variantCounter = new Dictionary<string, uint>();
            using(var writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(vcfFilePath), "variantPositions.csv")))
            {
                void AnalyzeVariant(VcfVariantEntry variantEntry, List<VcfMetadataEntry> metadata)
                {
                    if(!variantEntry.FilterResult.Pass)
                        return;
                    if(!variantCounter.ContainsKey(variantEntry.Chromosome))
                        variantCounter.Add(variantEntry.Chromosome, 0);
                    variantCounter[variantEntry.Chromosome]++;

                    writer.WriteLine($"{variantEntry.Chromosome};{variantEntry.Position}");
                };
                var result = vcfLoader.Load(vcfFilePath, AnalyzeVariant);
            }
            foreach (var kvp in variantCounter)
            {
                var chromosome = kvp.Key;
                var numberOfVariants = kvp.Value;
                if (ChromosomeSizes.ContainsKey(chromosome))
                {
                    var chromosomeSize = ChromosomeSizes[chromosome];
                    Console.WriteLine($"Chromosome {chromosome}: {numberOfVariants} variants out of {chromosomeSize} bp = {numberOfVariants*1e6/chromosomeSize:F0} ppm");
                }
                else
                {
                    Console.WriteLine($"Chromosome {chromosome}: {numberOfVariants} variants");
                }
            }
        }

        [Test]
        [TestCase(@"F:\datasets\mygenome\OtherGenomes\genome-1000.vcf")]
        public void Analyze1000GenomesVariantDistributions(string vcfFilePath)
        {
            var outputDirectory = Path.Combine(Path.GetDirectoryName(vcfFilePath), "VariantPositions");
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            foreach (var file in Directory.GetFiles(outputDirectory, "*.csv"))
            {
                File.Delete(file);
            }

            var vcfLoader = new VcfLoader();

            var genomeVariantPositions = new Dictionary<string, List<GenomePosition>>();
            void AnalyzeVariant(VcfVariantEntry variantEntry, List<VcfMetadataEntry> metadata)
            {
                var chromosome = int.Parse(variantEntry.Chromosome);
                //if(!Regex.IsMatch(variantEntry.Chromosome, "^chr[0-9XY]$"))
                //    return;
                //var chromosome = variantEntry.Chromosome == "chrX" ? 23
                //        : variantEntry.Chromosome == "chrY" ? 24
                //        : int.Parse(variantEntry.Chromosome.Substring(3));
                var baseNumber = int.Parse(variantEntry.Position);
                foreach (var kvp in variantEntry.OtherFields.Skip(1)) // Skip 1 for FORMAT column
                {
                    var genomeId = kvp.Key;
                    var hasGenomeVariant = kvp.Value != "0|0" && Regex.IsMatch(kvp.Value, "([1-9][0-9]*\\|[0-9]+|[0-9]+\\|[1-9][0-9]*)");
                    if (hasGenomeVariant)
                    {
                        if(!genomeVariantPositions.ContainsKey(genomeId))
                            genomeVariantPositions.Add(genomeId, new List<GenomePosition>());
                        var variantPositions = genomeVariantPositions[genomeId];
                        variantPositions.Add(new GenomePosition(chromosome, baseNumber));
                        if (variantPositions.Count > 10000)
                        {
                            File.AppendAllLines(
                                Path.Combine(outputDirectory, $"{genomeId}.csv"),
                                variantPositions.Select(position => $"{position.Chromosome};{position.BaseNumber}"));
                            variantPositions.Clear();
                        }
                    }
                }
            }
            var result = vcfLoader.Load(vcfFilePath, AnalyzeVariant);
            foreach (var kvp in genomeVariantPositions)
            {
                var genomeId = kvp.Key;
                var variantPositions = kvp.Value;
                File.AppendAllLines(
                    Path.Combine(outputDirectory, $"{genomeId}.csv"),
                    variantPositions.Select(position => $"{position.Chromosome};{position.BaseNumber}"));
            }
        }
        
        private static readonly Dictionary<string,uint> ChromosomeSizes = new()
        {
            { "chr1", 248956422 },
            { "chr2", 242193529 },
            { "chr3", 198295559 },
            { "chr4", 190214555 },
            { "chr5", 181538259 },
            { "chr6", 170805979 },
            { "chr7", 159345973 },
            { "chr8", 145138636 },
            { "chr9", 138394717 },
            { "chr10", 133797422 },
            { "chr11", 135086622 },
            { "chr12", 133275309 },
            { "chr13", 114364328 },
            { "chr14", 107043718 },
            { "chr15", 101991189 },
            { "chr16", 90338345 },
            { "chr17", 83257441 },
            { "chr18", 80373285 },
            { "chr19", 58617616 },
            { "chr20", 64444167 },
            { "chr21", 46709983 },
            { "chr22", 50818468 },
            { "chrX", 156040895 },
            { "chrY", 57227415 }
        };

        internal class GenomePosition
        {
            public int Chromosome { get; }
            public int BaseNumber { get; }

            public GenomePosition(int chromosome, int baseNumber)
            {
                Chromosome = chromosome;
                BaseNumber = baseNumber;
            }
        }
    }
}
