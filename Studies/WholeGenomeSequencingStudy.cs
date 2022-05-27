using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private const string ReferenceSequenceFilePath = @"F:\datasets\mygenome\references\nebula-hg38.fna";
        private const string GenePositionFilePath = @"F:\HumanGenome\gene_positions.csv";
        private const string GeneVariantDatabaseName = "GenVariantStatistics";

        [Test]
        public async Task ExportSelfExtractedVariantPositions()
        {
            var genomeVariantDb = new GeneVariantDb(GeneVariantDatabaseName);
            var lines = new List<string>();
            await foreach (var variant in genomeVariantDb.GetSequenceVariants())
            {
                var line = $"{variant.Chromosome.Replace("chr","").Replace("X", "23").Replace("Y", "24")};{variant.ReferenceStartIndex}";
                lines.Add(line);
            }
            await File.WriteAllLinesAsync(@"F:\datasets\mygenome\variantPositions_selfExtracted.csv", lines);
        }

        [Test]
        [TestCase("chr1")]
        [TestCase("chr2")]
        [TestCase("chr3")]
        [TestCase("chr4")]
        [TestCase("chr5")]
        [TestCase("chr6")]
        [TestCase("chr7")]
        [TestCase("chr8")]
        [TestCase("chr9")]
        [TestCase("chr10")]
        [TestCase("chr11")]
        [TestCase("chr12")]
        [TestCase("chr13")]
        [TestCase("chr14")]
        [TestCase("chr15")]
        [TestCase("chr16")]
        [TestCase("chr17")]
        [TestCase("chr18")]
        [TestCase("chr19")]
        [TestCase("chr20")]
        [TestCase("chr21")]
        [TestCase("chr22")]
        [TestCase("chrX")]
        [TestCase("chrY")]
        public async Task CallVariants(string chromosome)
        {
            const int ChunkSize = 30_000;
            const int VariantLengthSequenceCutoff = 100;
            var chromosomeSize = ChromosomeSizes[chromosome];
            var alignmentAccessorFactory = new CramGenomeSequenceAlignmentAccessorFactory(AlignmentFilePath, ReferenceSequenceFilePath);
            using var alignmentAccessor = alignmentAccessorFactory.Create();
            var genomeVariantDatabase = new GeneVariantDb(GeneVariantDatabaseName);
            var lastAddedVariantForChromosome = await genomeVariantDatabase.GetLastGenomeSequenceVariant(chromosome);
            var startIndex = lastAddedVariantForChromosome?.ReferenceEndIndex + 1 ?? 0;
            var isVariantInProgress = false;
            var variantStartIndex = 0;
            StringBuilder variantReferenceSequence = null;
            StringBuilder variantPrimarySequence = null;
            StringBuilder variantSecondarySequence = null;
            for (int chunkStartIndex = startIndex; chunkStartIndex < chromosomeSize; chunkStartIndex += ChunkSize)
            {
                var chunkEndIndex = Math.Min(chunkStartIndex + ChunkSize - 1, chromosomeSize-1);
                var alignment = alignmentAccessor.GetAlignment(chromosome, chunkStartIndex, chunkEndIndex);
                if(alignment.Reads.Count == 0)
                    continue;

                for (int referenceIndex = alignment.StartIndex; referenceIndex <= alignment.EndIndex; referenceIndex++)
                {
                    var localIndex = referenceIndex - alignment.StartIndex;
                    if(localIndex >= alignment.ReferenceSequence.Length || localIndex >= alignment.AlignmentSequence.PrimarySequence.Length)
                        break;
                    var referenceNucleotide = alignment.ReferenceSequence.GetBaseAtPosition(localIndex);
                    var primaryConsensusNucleotide = alignment.AlignmentSequence.PrimarySequence.GetBaseAtPosition(localIndex);
                    var secondaryConsensusNucleotide = alignment.AlignmentSequence.SecondarySequence.GetBaseAtPosition(localIndex);

                    var isDiff = referenceNucleotide != 'N' 
                                 && primaryConsensusNucleotide != 'N'
                                 && (primaryConsensusNucleotide != referenceNucleotide || secondaryConsensusNucleotide != referenceNucleotide);
                    if(!isDiff)
                    {
                        if (isVariantInProgress)
                        {
                            var variant = new GenomeSequenceVariant(
                                PersonId,
                                chromosome,
                                variantStartIndex,
                                referenceIndex-1,
                                variantReferenceSequence.ToString(),
                                variantPrimarySequence.ToString(),
                                variantSecondarySequence.ToString());
                            await genomeVariantDatabase.Store(variant);
                            isVariantInProgress = false;
                        }
                        continue;
                    }

                    if (!isVariantInProgress)
                    {
                        variantStartIndex = referenceIndex;
                        variantReferenceSequence = new StringBuilder();
                        variantPrimarySequence = new StringBuilder();
                        variantSecondarySequence = new StringBuilder();
                        isVariantInProgress = true;
                    }

                    if (referenceIndex - variantStartIndex < VariantLengthSequenceCutoff)
                    {
                        variantReferenceSequence.Append(referenceNucleotide);
                        variantPrimarySequence.Append(primaryConsensusNucleotide);
                        variantSecondarySequence.Append(secondaryConsensusNucleotide);
                    }
                    else if(variantReferenceSequence.Length > 0) // Clear sequences when cutoff is reached and don't include them at all in the database object
                    {
                        variantReferenceSequence.Clear();
                        variantPrimarySequence.Clear();
                        variantSecondarySequence.Clear();
                    }
                }
            }
        }

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
            //var sequenceNameTranslation = Enumerable.Range(1, 22).Select(x => x.ToString()).Concat(new[] { "X", "Y", "M" }).ToDictionary(x => $"chr{x}", x => x);
            //var vcfAccessor = new VcfAccessor(PersonId, VariantFilePath, sequenceNameTranslation);
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
            var lastChromosome = "";
            var chromosomeVariants = new List<GenomeSequenceVariant>();
            foreach (var genePosition in genePositions)
            {
                var unknownOriginVariants = new GeneVariantStatistics(PersonId, GeneParentalOrigin.Unknown, genePosition);
                //var parent1Variants = new GeneVariantStatistics(PersonId, GeneParentalOrigin.Parent1, genePosition);
                //var parent2Variants = new GeneVariantStatistics(PersonId, GeneParentalOrigin.Parent2, genePosition);
                //void AddVariantToGenes(VcfVariantEntry variant)
                //{
                //    var fieldNames = variant.OtherFields["FORMAT"].Split(':').ToList();
                //    var genoTypeIndex = fieldNames.FindIndex(x => x == "GT");
                //    var splittedValues = variant.OtherFields["NG1RLQNK6J"].Split(':');
                //    if (splittedValues.Length != fieldNames.Count)
                //        throw new Exception("Genotype and other information was in an unexpected format");
                //    var genoType = splittedValues[genoTypeIndex];
                //    var isPhased = genoType[1] == '|';
                //    var parent1HasVariant = genoType[0] == '1';
                //    var parent2HasVariant = genoType.Length == 3 && genoType[2] == '1';
                //    var isHeterogenous = parent1HasVariant != parent2HasVariant;
                //    if (isPhased || (parent1HasVariant && parent2HasVariant))
                //    {
                //        if (parent1HasVariant) 
                //            PopulationGenomeStudy.AddVariant(parent1Variants, variant, isHeterogenous);
                //        if (parent2HasVariant) 
                //            PopulationGenomeStudy.AddVariant(parent2Variants, variant, isHeterogenous);
                //    }
                //    else
                //    {
                //        PopulationGenomeStudy.AddVariant(unknownOriginVariants, variant, isHeterogenous);
                //    }
                //}

                //vcfAccessor.LoadInRange(genePosition, (variant, metadata) => AddVariantToGenes(variant));
                if (genePosition.Chromosome != lastChromosome)
                {
                    chromosomeVariants.Clear();
                    var chromosomeVariantIterator = geneVariantDb.GetSequenceVariants(x => x.Chromosome == "chr" + genePosition.Chromosome);
                    await foreach (var variant in chromosomeVariantIterator)
                    {
                        chromosomeVariants.Add(variant);
                    }
                }
                var geneVariants = chromosomeVariants.Where(x => 
                         x.ReferenceStartIndex <= genePosition.Position.To
                         && x.ReferenceEndIndex >= genePosition.Position.From);
                foreach (var variant in geneVariants)
                {
                    unknownOriginVariants.VariantCount++;
                    unknownOriginVariants.VariantPositions.Add(variant.ReferenceStartIndex);
                    unknownOriginVariants.TotalVariantLength += variant.ReferenceSequence.Length;

                    if (variant.IsHeterogenous)
                        unknownOriginVariants.HeterogenousCount++;

                    var isDeletion = variant.PrimaryVariantSequence.Contains("-") || variant.SecondaryVariantSequence.Contains("-");
                    if(isDeletion)
                    {
                        unknownOriginVariants.DeletionCount++;
                        unknownOriginVariants.DeletionLength += Math.Max(
                            variant.PrimaryVariantSequence.Count(x => x == '-'),
                            variant.SecondaryVariantSequence.Count(x => x == '-'));
                    }

                    var isSNP = variant.ReferenceSequence.Length == 1;
                    if (isSNP)
                    {
                        unknownOriginVariants.SNPCount++;
                    }
                }

                foreach (var geneVariantStatistic in new []{ /*parent1Variants, parent2Variants,*/ unknownOriginVariants })
                {
                    geneVariantStatistic.UpdateRatios();
                    WriteGeneVariantsToConsole(geneVariantStatistic);
                    outputLines.Add(FormatGeneVariantsAsCsv(geneVariantStatistic));
                    //await geneVariantDb.Store(geneVariantStatistic);
                }
                lastChromosome = genePosition.Chromosome;
            }
            await File.WriteAllLinesAsync(@"F:\datasets\mygenome\geneVariants_selfExtracted.csv", outputLines);
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
            var variantLoader = new VcfAccessor(PersonId, VariantFilePath);
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
            var variantLoader = new VcfAccessor(PersonId, VariantFilePath);

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
                var genomeId = variant.GetGenomeIds().Single();
                if (variant.IsHeterogenous(genomeId))
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

        [Test]
        [TestCase("COX5A")]
        public void GetReadsInRegion(string geneSymbol)
        {
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            var genePosition = genePositions.First(x => x.GeneSymbol == geneSymbol);
            GetReadsInRegion("chr" + genePosition.Chromosome, genePosition.Position.From, genePosition.Position.To, geneSymbol);
        }

        [Test]
        [TestCase("chr1", 31244000, 31245000)]
        [TestCase("chr7", 117479963, 117668665)]
        public void GetReadsInRegion(string chromosome, int startIndex, int endIndex, string geneSymbol = null)
        {
            var alignmentAccessorFactory = new CramGenomeSequenceAlignmentAccessorFactory(AlignmentFilePath, ReferenceSequenceFilePath);
            using var alignmentAccessor = alignmentAccessorFactory.Create();
            var alignment = alignmentAccessor.GetAlignment(chromosome, startIndex, endIndex);
            Console.WriteLine($">{alignment.Chromosome}:{alignment.StartIndex}:{alignment.EndIndex}");
            Console.WriteLine($"Read count: {alignment.Reads.Count}");
            Console.WriteLine();
            var outputDirectory = @"F:\datasets\mygenome\Alignments";
            var outputFilePath = geneSymbol != null
                ? Path.Combine(outputDirectory, $"{geneSymbol}.txt")
                : Path.Combine(outputDirectory, $"{chromosome}_{startIndex}_{endIndex}.txt");
            GenomeAlignmentPrinter.Print(alignment, GenomeAlignmentPrinter.PrintTarget.File, outputFilePath);
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
        public void AnalyzeVariantPositionDistribution()
        {
            var vcfLoader = new VcfAccessor(PersonId, VariantFilePath);
            var variantCounter = new Dictionary<string, uint>();
            using(var writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(VariantFilePath), "variantPositions.csv")))
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
                vcfLoader.Load(AnalyzeVariant);
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
        public void Analyze1000GenomesVariantDistributions()
        {
            const string Vcf1000GenomesFilePath = @"F:\datasets\mygenome\OtherGenomes\genome-1000.vcf";
            var outputDirectory = Path.Combine(Path.GetDirectoryName(Vcf1000GenomesFilePath), "VariantPositions");
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
            foreach (var file in Directory.GetFiles(outputDirectory, "*.csv"))
            {
                File.Delete(file);
            }

            var vcfLoader = new VcfAccessor(null, Vcf1000GenomesFilePath);

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
                foreach (var genomeId in variantEntry.GetGenomeIds())
                {
                    var hasGenomeVariant = variantEntry.HasPersonVariant(genomeId);
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
            vcfLoader.Load(AnalyzeVariant);
            foreach (var kvp in genomeVariantPositions)
            {
                var genomeId = kvp.Key;
                var variantPositions = kvp.Value;
                File.AppendAllLines(
                    Path.Combine(outputDirectory, $"{genomeId}.csv"),
                    variantPositions.Select(position => $"{position.Chromosome};{position.BaseNumber}"));
            }
        }
        
        private static readonly Dictionary<string,int> ChromosomeSizes = new()
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
