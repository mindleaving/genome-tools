using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        public const string PersonId = "JanScholtyssek";
        private const string VariantFilePath = @"F:\datasets\mygenome\genome-janscholtyssek.vcf";
        private const string AlignmentFilePath = @"F:\datasets\mygenome\genome-janscholtyssek.cram";
        private const string ReferenceSequenceFilePath = @"F:\datasets\mygenome\references\hg38.p13.fa";
        private const string GenePositionFilePath = @"F:\HumanGenome\gene_positions.csv";
        private const string GeneVariantDatabaseName = "GenVariantStatistics";

        [Test]
        public async Task VariantsComparedTo1000Genomes()
        {
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            var geneVariantDb = new GeneVariantDb(GeneVariantDatabaseName);
            var stopWatch = Stopwatch.StartNew();
            var manyVariantsCount = 0;
            var fewVariantsCount = 0;
            var normalVariantCount = 0;
            foreach (var genePosition in genePositions)
            {
                var myVariants = await geneVariantDb.GetGeneVariantStatistics(x => x.GeneSymbol == genePosition.GeneSymbol && x.PersonId == PersonId);
                if(!myVariants.Any())
                    return;
                var populationStatistics = await geneVariantDb.GetAggregatedGeneVariantStatistics(genePosition.GeneSymbol);
                var populationMetric = populationStatistics.VariantCount;
                var myMetric = myVariants.Sum(x => x.VariantCount) / 2.0;

                //if (myMetric < populationMetric.Percentile10)
                //    fewVariantsCount++;
                //else if (myMetric > populationMetric.Percentile90)
                //    manyVariantsCount++;
                //else
                //    normalVariantCount++;

                if (myMetric > populationMetric.Maximum)
                    Console.WriteLine($"{genePosition.GeneSymbol};{genePosition.Chromosome}:{genePosition.Position.From}:{genePosition.Position.To}");

                //var isOutlier = !new Range<double>(populationMetric.Percentile10, populationMetric.Percentile90).Contains(myMetric);
                //if (isOutlier)
                //    Console.WriteLine($"{genePosition.GeneSymbol};{genePosition.Chromosome}:{genePosition.Position.From}:{genePosition.Position.To}");

                //Console.WriteLine($"#### {genePosition.GeneSymbol} ####");
                //Console.WriteLine($"{PersonId} - Variants: {myMetric:F1}");
                //Console.WriteLine($"Population - Variants: {populationMetric.Median:F1} ({populationMetric.Minimum:F0}-[{populationMetric.Percentile10:F1}-{populationMetric.Percentile90:F1}]-{populationMetric.Maximum:F0})");
                //Console.WriteLine();
            }
            stopWatch.Stop();
            //Console.WriteLine($"Many variants: {manyVariantsCount}");
            //Console.WriteLine($"Few variants: {fewVariantsCount}");
            //Console.WriteLine($"Normal variants: {normalVariantCount}");
            Console.WriteLine($"Time: {stopWatch.Elapsed.TotalSeconds} s");
        }

        [Test]
        public async Task VariantsByGene()
        {
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            var sequenceNameTranslation = Enumerable.Range(1, 22).Select(x => x.ToString()).Concat(new[] { "X", "Y", "M" }).ToDictionary(x => $"chr{x}", x => x);
            var vcfAccessor = new VcfAccessor(VariantFilePath, sequenceNameTranslation);
            var geneVariantDb = new GeneVariantDb(GeneVariantDatabaseName);
            var outputLines = new List<string>
            {
                "Symbol;Chromosome;StartIndex;EndIndex;ParentalOrigin;"
                + "GeneLength;VariantCount;"
                + "HeterogenousCount;HeterogenousRatio;"
                + "DeletionCount;DeletionCountRatio;DeletionLength;DeletionLengthRatio;"
                + "InsertCount;InsertCountRatio;InsertLength;InsertLengthRatio;"
                + "SNPCount;SNPRatio"
            };
            foreach (var genePosition in genePositions.Take(1))
            {
                var unknownOriginVariants = new GeneVariantStatistics(PersonId, GeneParentalOrigin.Unknown, genePosition);
                var parent1Variants = new GeneVariantStatistics(PersonId, GeneParentalOrigin.Parent1, genePosition);
                var parent2Variants = new GeneVariantStatistics(PersonId, GeneParentalOrigin.Parent2, genePosition);
                void AddVariantToGenes(VcfVariantEntry variant)
                {
                    var fieldNames = variant.OtherFields["FORMAT"].Split(':').ToList();
                    var genoTypeIndex = fieldNames.FindIndex(x => x == "GT");
                    var splittedValues = variant.OtherFields["NG1RLQNK6J"].Split(':');
                    if (splittedValues.Length != fieldNames.Count)
                        throw new Exception("Genotype and other information was in an unexpected format");
                    var genoType = splittedValues[genoTypeIndex];
                    var isPhased = genoType[1] == '|';
                    var parent1HasVariant = genoType[0] == '1';
                    var parent2HasVariant = genoType.Length == 3 && genoType[2] == '1';
                    var isHeterogenous = parent1HasVariant != parent2HasVariant;
                    if (isPhased || (parent1HasVariant && parent2HasVariant))
                    {
                        if (parent1HasVariant) 
                            PopulationGenomeStudy.AddVariant(parent1Variants, variant, isHeterogenous);
                        if (parent2HasVariant) 
                            PopulationGenomeStudy.AddVariant(parent2Variants, variant, isHeterogenous);
                    }
                    else
                    {
                        PopulationGenomeStudy.AddVariant(unknownOriginVariants, variant, isHeterogenous);
                    }
                }

                try
                {
                    vcfAccessor.LoadInRange(genePosition, (variant, metadata) => AddVariantToGenes(variant));
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }

                foreach (var geneVariantStatistic in new []{ parent1Variants, parent2Variants, unknownOriginVariants })
                {
                    geneVariantStatistic.UpdateRatios();
                    WriteGeneVariantsToConsole(geneVariantStatistic);
                    outputLines.Add(FormatGeneVariantsAsCsv(geneVariantStatistic));
                    //await geneVariantDb.Store(geneVariantStatistic);
                }
            }
            await File.WriteAllLinesAsync(@"F:\datasets\mygenome\geneVariants.csv", outputLines);
        }

        private string FormatGeneVariantsAsCsv(GeneVariantStatistics geneVariantStatistics)
        {
            return $"{geneVariantStatistics.GeneSymbol};{geneVariantStatistics.Chromosome};{geneVariantStatistics.StartIndex};{geneVariantStatistics.EndIndex};{geneVariantStatistics.ParentalOrigin};" 
                   + $"{geneVariantStatistics.GeneLength};{geneVariantStatistics.VariantCount};"
                   + $"{geneVariantStatistics.HeterogenousCount};{FormatRatio(geneVariantStatistics.HeterogenousCount,geneVariantStatistics.VariantCount)};"
                   + $"{geneVariantStatistics.DeletionCount};{FormatRatio(geneVariantStatistics.DeletionCount, geneVariantStatistics.VariantCount)};{geneVariantStatistics.DeletionLength};{FormatRatio(geneVariantStatistics.DeletionLength, geneVariantStatistics.GeneLength)};"
                   + $"{geneVariantStatistics.InsertionCount};{FormatRatio(geneVariantStatistics.InsertionCount, geneVariantStatistics.VariantCount)};{geneVariantStatistics.InsertionLength};{FormatRatio(geneVariantStatistics.InsertionLength, geneVariantStatistics.GeneLength)};"
                   + $"{geneVariantStatistics.SNPCount};{FormatRatio(geneVariantStatistics.SNPCount, geneVariantStatistics.VariantCount)}";
        }

        private static void WriteGeneVariantsToConsole(GeneVariantStatistics geneVariantStatistics)
        {
            Console.WriteLine($"#### {geneVariantStatistics.GeneSymbol} ({geneVariantStatistics.Chromosome}:{geneVariantStatistics.StartIndex}:{geneVariantStatistics.EndIndex}) - {geneVariantStatistics.ParentalOrigin} ####");
            Console.WriteLine($"Gene length: {geneVariantStatistics.GeneLength}");
            if (geneVariantStatistics.VariantCount > 0)
            {
                Console.WriteLine($"Variants: {geneVariantStatistics.VariantCount} ({geneVariantStatistics.VariantCount * 1e6 / geneVariantStatistics.GeneLength:F0}ppm)");
                Console.WriteLine($"Deletions: {geneVariantStatistics.DeletionCount}, length: {geneVariantStatistics.DeletionLength} ({geneVariantStatistics.DeletionLength * 1e6 / geneVariantStatistics.GeneLength:F0}ppm)");
                Console.WriteLine($"Insertions: {geneVariantStatistics.InsertionCount}, length: {geneVariantStatistics.InsertionLength}  ({geneVariantStatistics.InsertionLength * 1e6 / geneVariantStatistics.GeneLength:F0}ppm)");
                Console.WriteLine($"Heterogenous: {geneVariantStatistics.HeterogenousCount} ({geneVariantStatistics.HeterogenousCount * 100 / geneVariantStatistics.VariantCount:F0}%)");
                Console.WriteLine($"SNPs: {geneVariantStatistics.SNPCount} ({geneVariantStatistics.SNPCount * 100 / geneVariantStatistics.VariantCount:F0}%)");
            }
            else
            {
                Console.WriteLine("No variants");
            }

            Console.WriteLine();
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
            var variantLoader = new VcfAccessor(VariantFilePath);
            var nonSnpVariants = new List<string>();

            void GetNonSnpVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadataEntries)
            {
                if(!variant.FilterResult.Pass)
                    return;
                if (variant.IsSNP) // Also discard "*" and "." alternates
                    return;
                nonSnpVariants.Add($"{variant.ReferenceBases}|{variant.AlternateBases}");
            }

            variantLoader.Load(GetNonSnpVariant);
            File.WriteAllLines(@"F:\datasets\mygenome\variantsNonSnpRefAlt.txt", nonSnpVariants);
        }

        [Test]
        public void VariantStatistics()
        {
            var variantLoader = new VcfAccessor(VariantFilePath);

            var variantCount = 0;
            var deletionCount = 0;
            var insertionCount = 0;
            var snpCount = 0;
            var heterogenousCount = 0;
            var multiBaseMismatchCount = 0;
            var lowQualityVariantCount = 0;
            var noAltVariantCount = 0;
            var missingAltVariantCount = 0;
            var phasedCount = 0;
            void AnalyzeVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadataEntries)
            {
                variantCount++;
                var isLowQuality = !variant.FilterResult.Pass;
                if (isLowQuality)
                {
                    lowQualityVariantCount++;
                    return;
                }
                if (IsHeterogenous(variant))
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

                var genoType = variant.OtherFields["NG1RLQNK6J"].Substring(0, 3);
                if (genoType[1] == '|')
                    phasedCount++;
                else if (genoType[1] != '/')
                    throw new Exception($"Unexpected genoType format: {genoType}");
            }
            variantLoader.Load(AnalyzeVariant);

            Console.WriteLine($"Total: {variantCount}");
            Console.WriteLine($"Phased: {phasedCount}");
            Console.WriteLine($"Low quality variants: {lowQualityVariantCount}");
            Console.WriteLine($"Deletions: {deletionCount}");
            Console.WriteLine($"Insertions: {insertionCount}");
            Console.WriteLine($"Heterogenous: {heterogenousCount}");
            Console.WriteLine($"SNPs: {snpCount}");
            Console.WriteLine($"MultiNPs: {multiBaseMismatchCount}");
            Console.WriteLine($"Missing: {missingAltVariantCount}");
            Console.WriteLine($"No alternative: {noAltVariantCount}");
        }

        private static bool IsHeterogenous(VcfVariantEntry variant)
        {
            var fieldNames = variant.OtherFields["FORMAT"].Split(':').ToList();
            var genoTypeIndex = fieldNames.FindIndex(x => x == "GT");
            var splittedValues = variant.OtherFields["NG1RLQNK6J"].Split(':');
            if (splittedValues.Length != fieldNames.Count)
                throw new Exception("Genotype and other information was in an unexpected format");
            var genoType = splittedValues[genoTypeIndex];
            var isPhased = genoType[1] == '|';
            var parent1HasVariant = genoType[0] == '1';
            var parent2HasVariant = genoType.Length == 3 && genoType[2] == '1';
            return parent1HasVariant != parent2HasVariant;
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
            var vcfLoader = new VcfAccessor(vcfFilePath);
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
                var result = vcfLoader.Load(AnalyzeVariant);
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

            var vcfLoader = new VcfAccessor(vcfFilePath);

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
            var result = vcfLoader.Load(AnalyzeVariant);
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
