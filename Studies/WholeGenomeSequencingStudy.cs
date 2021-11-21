using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;
using Commons.Mathematics;
using GenomeTools.ChemistryLibrary.IO.Cram;
using GenomeTools.ChemistryLibrary.IO.Cram.Index;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using GenomeTools.Studies.GenomeAnalysis;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    public class WholeGenomeSequencingStudy
    {
        private const string PersonId = "JanScholtyssek";
        private const string VariantFilePath = @"F:\datasets\mygenome\genome-janscholtyssek.vcf";
        private const string AlignmentFilePath = @"F:\datasets\mygenome\genome-janscholtyssek.cram";
        private const string ReferenceSequenceFilePath = @"F:\datasets\mygenome\references\hg38.p13.fa";
        private const string GenePositionFilePath = @"F:\HumanGenome\gene_positions.csv";

        [Test]
        public void VariantsByGene()
        {
            var genePositions = GenePositionStudy.ReadGenePositions(@"F:\HumanGenome\gene_positions.csv");
            var genesWithVariants = genePositions
                .Select(x => new 
                {
                    x.GeneSymbol,
                    x.Chromosome, 
                    x.Position,
                    Variants = new List<VcfVariantEntry>()
                })
                .GroupBy(x => x.Chromosome)
                .ToDictionary(x => x.Key, x => x);
            var vcfLoader = new VcfLoader();

            void AddVariantToGenes(VcfVariantEntry variant)
            {
                if (!genesWithVariants.ContainsKey(variant.Chromosome))
                    return;
                var variantRange = new Range<int>(variant.Position, variant.Position + variant.ReferenceBases.Length - 1);
                var matchingGenes = genesWithVariants[variant.Chromosome].Where(x => x.Position.Overlaps(variantRange));
                matchingGenes.ForEach(x => x.Variants.Add(variant));
            }
            var sequenceNameTranslation = Enumerable.Range(1, 22).Select(x => x.ToString()).Concat(new[] { "X", "Y", "M" }).ToDictionary(x => $"chr{x}", x => x);
            vcfLoader.Load(VariantFilePath, (variant, metadata) => AddVariantToGenes(variant), sequenceNameTranslation);

            var outputLines = new List<string>
            {
                "Symbol;Chromosome;StartIndex;EndIndex;"
                + "GeneLength;VariantCount;"
                + "HeterogenousCount;HeterogenousRatio;"
                + "DeletionCount;DeletionCountRatio;DeletionLength;DeletionLengthRatio;"
                + "InsertCount;InsertCountRatio;InsertLength;InsertLengthRatio;"
                + "SNPCount;SNPRatio"
            };
            foreach (var geneWithVariants in genesWithVariants.Values.SelectMany(x => x))
            {
                var gene = new GeneVariantStatistics(
                    PersonId, 
                    GeneParentalOrigin.Both,
                    new GenePosition(geneWithVariants.GeneSymbol, geneWithVariants.Chromosome, geneWithVariants.Position), 
                    geneWithVariants.Variants);
                Console.WriteLine($"#### {gene.GeneSymbol} ({gene.Chromosome}:{gene.StartIndex}:{gene.EndIndex} ####");
                Console.WriteLine($"Gene length: {gene.GeneLength}");
                if (gene.VariantCount > 0)
                {
                    Console.WriteLine($"Variants: {gene.VariantCount} ({gene.VariantCount*1e6/gene.GeneLength:F0}ppm)");
                    Console.WriteLine($"Deletions: {gene.DeletionCount}, length: {gene.DeletionLength} ({gene.DeletionLength*1e6/gene.GeneLength:F0}ppm)");
                    Console.WriteLine($"Insertions: {gene.InsertionCount}, length: {gene.InsertionLength}  ({gene.InsertionLength*1e6/gene.GeneLength:F0}ppm)");
                    Console.WriteLine($"Heterogenous: {gene.HeterogenousCount} ({gene.HeterogenousCount*100/gene.VariantCount:F0}%)");
                    Console.WriteLine($"SNPs: {gene.SNPCount} ({gene.SNPCount*100/gene.VariantCount:F0}%)");
                }
                else
                {
                    Console.WriteLine("No variants");
                }
                Console.WriteLine();

                outputLines.Add($"{gene.GeneSymbol};{gene.Chromosome};{gene.StartIndex};{gene.EndIndex};" 
                                + $"{gene.GeneLength};{gene.VariantCount};"
                                + $"{gene.HeterogenousCount};{FormatRatio(gene.HeterogenousCount,gene.VariantCount)};"
                                + $"{gene.DeletionCount};{FormatRatio(gene.DeletionCount, gene.VariantCount)};{gene.DeletionLength};{FormatRatio(gene.DeletionLength, gene.GeneLength)};"
                                + $"{gene.InsertionCount};{FormatRatio(gene.InsertionCount, gene.VariantCount)};{gene.InsertionLength};{FormatRatio(gene.InsertionLength, gene.GeneLength)};"
                                + $"{gene.SNPCount};{FormatRatio(gene.SNPCount, gene.VariantCount)}");
            }
            File.WriteAllLines(@"F:\datasets\mygenome\geneVariants.csv", outputLines);
        }

        private string FormatRatio(int nominator, int denominator)
        {
            if (denominator == 0)
                return "";
            var ratio = nominator / (double)denominator;
            return ratio.ToString("F4");
            if (ratio * 100 < 1)
                return (ratio * 1e6).ToString("F0");
            return (ratio * 100).ToString("F2");
        }

        [Test]
        public void VariantRefAltExploration()
        {
            var variantLoader = new VcfLoader();
            var nonSnpVariants = new List<string>();

            void GetNonSnpVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadataEntries)
            {
                if(!variant.FilterResult.Pass)
                    return;
                if (variant.IsSNP) // Also discard "*" and "." alternates
                    return;
                nonSnpVariants.Add($"{variant.ReferenceBases}|{variant.AlternateBases}");
            }

            variantLoader.Load(VariantFilePath, GetNonSnpVariant);
            File.WriteAllLines(@"F:\datasets\mygenome\variantsNonSnpRefAlt.txt", nonSnpVariants);
        }

        [Test]
        public void VariantStatistics()
        {
            var variantLoader = new VcfLoader();

            var variantCount = 0;
            var deletionCount = 0;
            var insertionCount = 0;
            var snpCount = 0;
            var heterogenousCount = 0;
            var multiBaseMismatchCount = 0;
            var lowQualityVariantCount = 0;
            var noAltVariantCount = 0;
            var missingAltVariantCount = 0;
            void AnalyzeVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadataEntries)
            {
                variantCount++;
                var isLowQuality = !variant.FilterResult.Pass;
                if (isLowQuality)
                {
                    lowQualityVariantCount++;
                    return;
                }
                if (variant.IsHeterogenous)
                    heterogenousCount++;

                foreach (var alternativeBase in variant.AlternateBases)
                {
                    if(alternativeBase == "*")
                    {
                        missingAltVariantCount++;
                        continue;
                    }

                    if (alternativeBase == ".")
                    {
                        noAltVariantCount++;
                        continue;
                    }
                    var alternateBasesLength = alternativeBase.Length;
                    var isSnp = variant.ReferenceBases.Length == 1 && alternateBasesLength == 1;
                    if (isSnp) 
                        snpCount++;
                    else
                    {
                        var isInsert = alternateBasesLength > variant.ReferenceBases.Length;
                        if (isInsert)
                            insertionCount++;
                        var isDeletion = variant.ReferenceBases.Length > alternateBasesLength;
                        if (isDeletion)
                            deletionCount++;
                        if (!isInsert && !isDeletion)
                            multiBaseMismatchCount++;
                    }
                }
            }
            variantLoader.Load(VariantFilePath, AnalyzeVariant);

            Console.WriteLine($"Total: {variantCount}");
            Console.WriteLine($"Low quality variants: {lowQualityVariantCount}");
            Console.WriteLine($"Deletions: {deletionCount}");
            Console.WriteLine($"Insertions: {insertionCount}");
            Console.WriteLine($"Heterogenous: {heterogenousCount}");
            Console.WriteLine($"SNPs: {snpCount}");
            Console.WriteLine($"MultiNPs: {multiBaseMismatchCount}");
            Console.WriteLine($"Missing: {missingAltVariantCount}");
            Console.WriteLine($"No alternative: {noAltVariantCount}");
        }

        [Test]
        [TestCase("chr1", 31244000, 31245000)]
        [TestCase("chr7", 117479963, 117668665)]
        public void GetReadsInRegion(string chromosome, int startIndex, int endIndex)
        {
            var cramHeaderReader = new CramHeaderReader(CramHeaderReader.Md5CheckFailureMode.WriteToConsole);
            var cramHeader = cramHeaderReader.Read(AlignmentFilePath, ReferenceSequenceFilePath);
            var referenceSequenceMap = ReferenceSequenceMap.FromSamHeaderEntries(cramHeader.SamHeader);
            using var alignmentAccessor = new CramGenomeSequenceAlignmentAccessor(AlignmentFilePath, ReferenceSequenceFilePath, referenceSequenceMap);
            var alignment = alignmentAccessor.GetAlignment(chromosome, startIndex, endIndex);
            Console.WriteLine($">{alignment.Chromosome}:{alignment.StartIndex}:{alignment.EndIndex}");
            Console.WriteLine($"Read count: {alignment.Reads.Count}");
            Console.WriteLine();
            GenomeAlignmentPrinter.Print(alignment, GenomeAlignmentPrinter.PrintTarget.File, $@"F:\datasets\mygenome\{chromosome}_{startIndex}_{endIndex}.txt");
        }

        [Test]
        public void ReadAllSlices()
        {
            var index = new CramIndexLoader().Load(AlignmentFilePath + ".crai");
            var cramHeaderReader = new CramHeaderReader(CramHeaderReader.Md5CheckFailureMode.WriteToConsole);
            var cramHeader = cramHeaderReader.Read(AlignmentFilePath, ReferenceSequenceFilePath);
            var referenceSequenceMap = ReferenceSequenceMap.FromSamHeaderEntries(cramHeader.SamHeader);
            using var alignmentAccessor = new CramGenomeSequenceAlignmentAccessor(AlignmentFilePath, ReferenceSequenceFilePath, referenceSequenceMap);
            foreach (var indexEntry in index.GetAll())
            {
                try
                {
                    var alignment = alignmentAccessor.GetAlignment(
                        referenceSequenceMap.GetSequenceNameFromIndex(indexEntry.ReferenceSequenceId),
                        indexEntry.AlignmentStart,
                        indexEntry.AlignmentStart + indexEntry.AlignmentSpan - 1);
                    Console.WriteLine($"Chromosome {alignment.Chromosome} " 
                                      + $"from {alignment.StartIndex} "
                                      + $"to {alignment.EndIndex} "
                                      + "successfully read");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not read alignment for " 
                                      + $"reference ID {indexEntry.ReferenceSequenceId} "
                                      + $"from {indexEntry.AlignmentStart} "
                                      + $"to {indexEntry.AlignmentStart + indexEntry.AlignmentSpan - 1}: "
                                      + $"{e.Message}");
                }
            }
        }

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
                var chromosome = variantEntry.Chromosome;
                //if(!Regex.IsMatch(variantEntry.Chromosome, "^chr[0-9XY]$"))
                //    return;
                //var chromosome = variantEntry.Chromosome == "chrX" ? 23
                //        : variantEntry.Chromosome == "chrY" ? 24
                //        : int.Parse(variantEntry.Chromosome.Substring(3));
                var baseNumber = variantEntry.Position;
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

        private class GenomePosition
        {
            public string Chromosome { get; }
            public int BaseNumber { get; }

            public GenomePosition(string chromosome, int baseNumber)
            {
                Chromosome = chromosome;
                BaseNumber = baseNumber;
            }
        }
    }
}
