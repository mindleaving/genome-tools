using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.Mathematics;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using GenomeTools.ChemistryLibrary.Objects;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    /// <summary>
    /// Extract and analyze genome variants that overlap with exons and hence influence the gene product
    /// </summary>
    public class ExonVariantStudy
    {
        private const string PersonId = "JanScholtyssek";
        private const string VariantFilePath = @"F:\datasets\mygenome\genome-janscholtyssek.vcf";
        private const string ExonPositionFilePath = @"F:\HumanGenome\exonBoundaryData\exonPositions_ucsc-refseq_%ASSEMBLY%";
        private const string GenePositionFilePath = @"F:\HumanGenome\gene_positions.csv";
        private const string Vcf1000GenomesFilePath = @"F:\datasets\mygenome\OtherGenomes\genome-1000_%CHROMOSOME%.vcf";


        [Test]
        public async Task ExtractVariantsOverlappingExonsForMyGenome()
        {
            var outputFilePath = @"F:\datasets\mygenome\variants_in_exons.csv";
            var vcfLoader = new ParallelizedVcfAccessor(PersonId, VariantFilePath);
            var genesWithExons = LoadExons(ExonPositionFilePath.Replace("%ASSEMBLY%", "hg38"));
            var maxGeneLength = genesWithExons.Values.Max(gene => gene.EndBase - gene.StartBase + 1);
            var chromosomeGenes = genesWithExons.Values
                .GroupBy(gene => gene.Chromosome)
                .ToDictionary(g => g.Key, g => new Queue<GeneLocationInfo>(g.OrderBy(x => x.EndBase)));
            var affectedGenes = new Dictionary<string,PersonExonVariantStatistics>();
            var output = new List<string> { "gene_symbol;transcript;chromosome;variant_position;is_heterogenous;reference;alternate" };
            var exonVariants = new List<ExonVariant>();
            void AnalyzeVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadata)
            {
                if(!chromosomeGenes.ContainsKey(variant.Chromosome))
                    return;
                var relevantGenes = chromosomeGenes[variant.Chromosome];
                while (relevantGenes.Count > 0 && relevantGenes.Peek().EndBase < variant.Position)
                {
                    relevantGenes.Dequeue();
                }
                var overlappingGenes = relevantGenes
                    .TakeWhile(gene => gene.EndBase <= variant.Position + variant.ReferenceBases.Length + maxGeneLength)
                    .Where(gene => IsOverlapping(gene, variant));
                var genomeId = variant.GetGenomeIds().Single();
                foreach (var gene in overlappingGenes)
                {
                    var overlappingExon = gene.Exons.FirstOrDefault(exon => IsOverlapping(exon, variant));
                    if (overlappingExon == null) 
                        continue;
                    if(!affectedGenes.ContainsKey(gene.TranscriptName))
                    {
                        affectedGenes.Add(gene.TranscriptName, new PersonExonVariantStatistics
                        {
                            PersonId = PersonId,
                            GeneSymbol = gene.GeneSymbol,
                            TranscriptName = gene.TranscriptName,
                            VariantCount = 0
                        });
                    }
                    affectedGenes[gene.TranscriptName].VariantCount++;
                    exonVariants.Add(new ExonVariant
                    {
                        Chromosome = variant.Chromosome,
                        Position = variant.Position,
                        GeneSymbol = gene.GeneSymbol,
                        TranscriptName = gene.TranscriptName,
                        IsHeterogenous = variant.IsHeterogenous(genomeId),
                        ReferenceBases = variant.ReferenceBases,
                        AlternateBases = variant.AlternateBases
                    });
                }
            }
            await vcfLoader.Load(AnalyzeVariant);

            //await vcfLoader.Load(AnalyzeVariant, variantFilter: entry => entry.Chromosome == "chr7", stopCriteria: entry => entry.Chromosome == "chr8");
            //var cftrVariants = exonVariants.Where(x => x.GeneSymbol == "CFTR").ToList();
            //Console.WriteLine($"CFTR variant positions: {string.Join(", ", cftrVariants.Select(x => $"{x.Position}({(x.IsHeterogenous ? "0|1" : "1|1")})"))}");
            //return;

            var orderedExonVariants = exonVariants
                .OrderBy(x => x.GeneSymbol)
                .ThenBy(x => x.TranscriptName)
                .ThenBy(x => x.Chromosome)
                .ThenBy(x => x.Position);
            foreach (var variant in orderedExonVariants)
            {
                output.Add(
                    $"{variant.GeneSymbol};"
                    + $"{variant.TranscriptName};"
                    + $"{variant.Chromosome};"
                    + $"{variant.Position};"
                    + $"{variant.IsHeterogenous};"
                    + $"{variant.ReferenceBases};"
                    + $"{string.Join("|", variant.AlternateBases)}");
            }
            await File.WriteAllLinesAsync(outputFilePath, output);

            output.Clear();
            var geneGroups = affectedGenes.Values
                .GroupBy(x => x.GeneSymbol)
                .OrderBy(x => x.Key);
            foreach (var geneGroup in geneGroups)
            {
                var transcriptGroups = geneGroup
                    .GroupBy(x => x.TranscriptName)
                    .OrderBy(x => x.Key);
                foreach (var transcriptGroup in transcriptGroups)
                {
                    var variantCount = transcriptGroup.Single().VariantCount;
                    output.Add($"{geneGroup.Key};{transcriptGroup.Key};{variantCount}");
                }
            }
            outputFilePath = @"F:\datasets\mygenome\variants_in_exons_statistics.csv";
            await File.WriteAllLinesAsync(outputFilePath, output);
        }

        private class ExonVariant
        {
            public string Chromosome { get; set; }
            public int Position { get; set; }
            public string GeneSymbol { get; set; }
            public string TranscriptName { get; set; }
            public bool IsHeterogenous { get; set; }
            public string ReferenceBases { get; set; }
            public IList<string> AlternateBases { get; set; }
        }

        [Test]
        [Parallelizable]
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
        [TestCase("chrMT")]
        public void ExtractVariantsOverlappingExonsFor1000Genomes(string chromosome)
        {
            var outputFilePath = $@"F:\datasets\mygenome\variants_in_exons_1000genomes_{chromosome}_statistics.csv";
            var vcfLoader = new VcfAccessor(PersonId, Vcf1000GenomesFilePath.Replace("%CHROMOSOME%", chromosome));
            var genesWithExons = LoadExons(ExonPositionFilePath.Replace("%ASSEMBLY%", "hg19"));
            var chromosomeGenes = new Queue<GeneLocationInfo>(genesWithExons.Values.Where(gene => gene.Chromosome == chromosome).OrderBy(x => x.EndBase));
            var maxGeneLength = chromosomeGenes.Max(gene => gene.EndBase - gene.StartBase + 1);
            var personExonVariantStatistics = new Dictionary<string, PersonExonVariantStatistics>();

            var variantCount = 0;
            void AnalyzeVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadata)
            {
                while (chromosomeGenes.Count > 0 && chromosomeGenes.Peek().EndBase < variant.Position)
                {
                    chromosomeGenes.Dequeue();
                }
                var overlappingGenes = chromosomeGenes
                    .TakeWhile(gene => gene.EndBase <= variant.Position + variant.ReferenceBases.Length + maxGeneLength)
                    .Where(gene => IsOverlapping(gene, variant))
                    .ToList();
                foreach (var gene in overlappingGenes)
                {
                    var overlappingExon = gene.Exons.FirstOrDefault(exon => IsOverlapping(exon, variant));
                    if (overlappingExon == null) 
                        continue;
                    variantCount++;
                    foreach (var genomeId in variant.GetGenomeIds())
                    {
                        var hasVariant = variant.HasPersonVariant(genomeId);
                        if(!hasVariant)
                            continue;
                        var statisticsKey = $"{genomeId}_{gene.TranscriptName}";
                        if(!personExonVariantStatistics.ContainsKey(statisticsKey))
                        {
                            personExonVariantStatistics.Add(statisticsKey, new PersonExonVariantStatistics
                            {
                                PersonId = genomeId,
                                GeneSymbol = gene.GeneSymbol,
                                TranscriptName = gene.TranscriptName,
                                VariantCount = 0
                            });
                        }

                        var statistics = personExonVariantStatistics[statisticsKey];
                        statistics.VariantCount++;
                    }
                }
            }
            vcfLoader.Load(AnalyzeVariant);
            Console.WriteLine($"Finished loading variants for {chromosome}");

            var personIds = personExonVariantStatistics.Values.Select(gene => gene.PersonId).Distinct().ToList();
            var output = new List<string>
            {
                $"gene_symbol;transcript;average;median;{string.Join(";", personIds)}"
            };
            var geneGroups = personExonVariantStatistics.Values
                .GroupBy(gene => gene.GeneSymbol)
                .OrderBy(x => x.Key);
            foreach (var geneGroup in geneGroups)
            {
                var transcriptGroups = geneGroup
                    .GroupBy(x => x.TranscriptName)
                    .OrderBy(x => x.Key);
                foreach (var transcriptGroup in transcriptGroups)
                {
                    var transcriptStatistics = transcriptGroup.ToDictionary(gene => gene.PersonId);
                    var personStatistics = personIds
                        .Select(personId => 
                            transcriptStatistics.ContainsKey(personId) 
                            ? transcriptStatistics[personId].VariantCount 
                            : 0
                        )
                        .ToList();
                    var averageVariantCount = personStatistics.Average();
                    var medianVariantCount = personStatistics.Select(x => (double)x).Median();
                    output.Add($"{geneGroup.Key};{transcriptGroup.Key};{averageVariantCount:F2};{medianVariantCount:F1};{string.Join(";", personStatistics)}");
                }
            }
            File.WriteAllLines(outputFilePath, output);
        }

        [Test]
        [TestCase("janscholtyssek")]
        [TestCase("NA18994")]
        [TestCase("HG03634")]
        public void CompareMyVariantsTo1000Genomes(string personId)
        {
            var myVariantsFilePath = $@"F:\datasets\mygenome\variants_in_exons_{personId}_statistics.csv";
            var myVariants = ReadMyVariants(myVariantsFilePath);
            var chromosomes = GetChromosomes();
            foreach (var chromosome in chromosomes)
            {
                var myChromosomeVariants = myVariants.Where(x => x.TranscriptName.StartsWith($"{chromosome}_")).ToList();
                var populationFilePath = $@"F:\datasets\mygenome\variants_in_exons_1000genomes_{chromosome}_statistics.csv";
                var populationVariants = ReadPopulationVariants(populationFilePath).ToDictionary(x => x.TranscriptName);
                var unmatchedCount = 0;
                var matchedCount = 0;
                var unusualVariants = new List<PersonExonVariantStatistics>();
                foreach (var myChromosomeVariant in myChromosomeVariants)
                {
                    if (!populationVariants.ContainsKey(myChromosomeVariant.TranscriptName))
                    {
                        unmatchedCount++;
                        continue;
                    }

                    var matchingPopulationStatistics = populationVariants[myChromosomeVariant.TranscriptName];
                    matchedCount++;
                    var percentile = CalculatePercentile(
                        matchingPopulationStatistics.PersonStatistics.Select(x => x.VariantCount).ToList(), 
                        myChromosomeVariant.VariantCount);
                    myChromosomeVariant.PopulationPercentile = percentile;
                    if (percentile > 95)
                    {
                        unusualVariants.Add(myChromosomeVariant);
                    }
                }

                Console.WriteLine($"Chromosome {chromosome}");
                Console.WriteLine("-----------------------");
                Console.WriteLine($"Matched: {matchedCount}");
                Console.WriteLine($"Unmatched: {unmatchedCount}");
                Console.WriteLine($"Unusual: {unusualVariants.Count}");
                foreach (var unusualVariant in unusualVariants.OrderByDescending(x => x.PopulationPercentile))
                {
                    Console.WriteLine($"- {unusualVariant.GeneSymbol} / {unusualVariant.TranscriptName}: {unusualVariant.PopulationPercentile:F1}% ({unusualVariant.VariantCount})");
                }
                Console.WriteLine();
            }
            
            var averagePercentile = myVariants
                .Where(x => x.PopulationPercentile.HasValue)
                .Average(x => x.PopulationPercentile.Value);
            Console.WriteLine($"Average percentile: {averagePercentile}");
            var medianPercentile = myVariants
                .Where(x => x.PopulationPercentile.HasValue)
                .Select(x => x.PopulationPercentile.Value)
                .Median();
            Console.WriteLine($"Median percentile: {medianPercentile}");

        }

        private static IEnumerable<string> GetChromosomes()
        {
            var chromosomes = Enumerable.Range(1, 22).Select(i => $"chr{i}").Concat(new[] { "chrX", "chrY" });
            return chromosomes;
        }

        [Test]
        [TestCase("NA18994")]
        [TestCase("HG03634")]
        public void ExtractVariantCountForPerson(string personId)
        {
            var outputFilePath = $@"F:\datasets\mygenome\variants_in_exons_{personId}_statistics.csv";
            var output = new List<string>();
            foreach (var chromosome in GetChromosomes())
            {
                var populationFilePath = $@"F:\datasets\mygenome\variants_in_exons_1000genomes_{chromosome}_statistics.csv";
                var populationVariantStatistics = ReadPopulationVariants(populationFilePath);
                foreach (var populationVariantStatistic in populationVariantStatistics)
                {
                    var personStatistics = populationVariantStatistic.PersonStatistics.Find(x => x.PersonId == personId);
                    if(personStatistics == null)
                        continue;
                    if(personStatistics.VariantCount == 0)
                        continue;
                    output.Add($"{personStatistics.GeneSymbol};{personStatistics.TranscriptName};{personStatistics.VariantCount}");
                }
            }
            File.WriteAllLines(outputFilePath, output);
        }

        // TODO: Move to Commons
        private static double CalculatePercentile(
            List<int> populationVariantCounts,
            int myVariantCount)
        {
            var firstIndex = populationVariantCounts.BinarySearch(myVariantCount);
            if (firstIndex < 0)
                firstIndex = ~firstIndex;
            return 100 * firstIndex / (double)populationVariantCounts.Count;
        }

        // TODO: Move to Commons
        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 10)]
        [TestCase(2, 20)]
        [TestCase(14, 90)]
        [TestCase(15, 100)]
        public void PercentileAsExpected(int input, double expected)
        {
            var values = new List<int> { 1, 1, 3, 4, 6, 10, 11, 11, 11, 14 };
            var actual = CalculatePercentile(values, input);
            Assert.That(actual, Is.EqualTo(expected).Within(1e-6));
        }

        private List<PopulationExonVariantStatistics> ReadPopulationVariants(
            string filePath)
        {
            using var reader = new StreamReader(filePath);
            var header = reader.ReadLine().Split(';'); // Skip header
            var variants = new List<PopulationExonVariantStatistics>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var splittedLine = line.Split(';');
                var geneSymbol = splittedLine[0];
                var transcriptName = splittedLine[1];
                var average = double.Parse(splittedLine[2]);
                var median = double.Parse(splittedLine[3]);
                var personStatistics = new List<PersonExonVariantStatistics>();
                for (int personIndex = 0; personIndex < header.Length-4; personIndex++)
                {
                    var personId = header[personIndex+4];
                    var personVariantCount = int.Parse(splittedLine[personIndex + 4]);
                    var exonVariant = new PersonExonVariantStatistics
                    {
                        PersonId = personId,
                        GeneSymbol = geneSymbol,
                        TranscriptName = transcriptName,
                        VariantCount = personVariantCount
                    };
                    personStatistics.Add(exonVariant);
                }

                var variantCounts = personStatistics.Select(x => (double)x.VariantCount).OrderBy(x => x).ToList();
                var std = StdDevAggregator.Calculate(variantCounts).PopulationStddev;
                var percentile90 = variantCounts[(int)Math.Floor(0.9 * variantCounts.Count)];
                var populationStatistics = new PopulationExonVariantStatistics
                {
                    GeneSymbol = geneSymbol,
                    TranscriptName = transcriptName,
                    AverageVariantCount = average,
                    StandardDeviationVariantCount = std,
                    MedianVariantCount = median,
                    Percentile90VariantCount = percentile90,
                    PersonStatistics = personStatistics.OrderBy(x => x.VariantCount).ToList()
                };
                variants.Add(populationStatistics);
            }
            return variants;
        }

        private List<PersonExonVariantStatistics> ReadMyVariants(
            string filePath)
        {
            using var reader = new StreamReader(filePath);
            //reader.ReadLine(); // Skip header
            var variants = new List<PersonExonVariantStatistics>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var splittedLine = line.Split(';');
                var geneSymbol = splittedLine[0];
                var transcriptName = splittedLine[1];
                var variantCount = int.Parse(splittedLine[2]);
                var exonVariant = new PersonExonVariantStatistics
                {
                    PersonId = PersonId,
                    GeneSymbol = geneSymbol,
                    TranscriptName = transcriptName,
                    VariantCount = variantCount
                };
                variants.Add(exonVariant);
            }
            return variants;
        }

        private bool IsOverlapping(
            ExonInfo exon,
            VcfVariantEntry variant)
        {
            if (exon.EndBase < variant.Position)
                return false;
            if (exon.StartBase > variant.Position + variant.ReferenceBases.Length - 1)
                return false;
            return true;
        }

        private bool IsOverlapping(
            GeneLocationInfo gene,
            VcfVariantEntry variant)
        {
            if (gene.EndBase < variant.Position)
                return false;
            if (gene.StartBase > variant.Position + variant.ReferenceBases.Length - 1)
                return false;
            return true;
        }

        private Dictionary<string, GeneLocationInfo> LoadExons(string exonPositionFilePath)
        {
            using var streamReader = new StreamReader(exonPositionFilePath);
            var genes = new Dictionary<string, GeneLocationInfo>();
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                var splittedLine = line.Split('\t');
                if(splittedLine.Length != 6)
                    continue;
                var chromosome = splittedLine[0];
                var exonStart = int.Parse(splittedLine[1]);
                var exonEnd = int.Parse(splittedLine[2]);
                var exonName = splittedLine[3];
                var transcriptProductName = chromosome + "_" + string.Join("_", exonName.Split('_').Take(2));
                var unknownField1 = splittedLine[4];
                var strand = splittedLine[5];
                if(!genes.ContainsKey(transcriptProductName))
                {
                    genes.Add(transcriptProductName, new GeneLocationInfo
                    {
                        TranscriptName = transcriptProductName,
                        Chromosome = chromosome,
                        StartBase = exonStart,
                        EndBase = exonEnd,
                        Strand = strand == "+" ? ReferenceDnaStrandRole.Plus : ReferenceDnaStrandRole.Minus,
                        Exons = new List<ExonInfo>()
                    });
                }
                var gene = genes[transcriptProductName];
                gene.Exons.Add(new ExonInfo
                {
                    StartBase = exonStart,
                    EndBase = exonEnd
                });
                if(exonEnd > gene.EndBase)
                    gene.EndBase = exonEnd;
                if(exonStart < gene.StartBase)
                    gene.StartBase = exonStart;
            }

            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath)
                .GroupBy(x => x.Chromosome)
                .ToDictionary(g => "chr" + g.Key, g => g.ToList());
            foreach (var gene in genes.Values)
            {
                if(!genePositions.ContainsKey(gene.Chromosome))
                    continue;
                var chromosomeGenePositions = genePositions[gene.Chromosome];
                var overlappingGenePositions = chromosomeGenePositions
                    .Where(genePosition => IsOverlapping(gene, genePosition))
                    .ToList();
                if(overlappingGenePositions.Count == 0)
                    continue;
                var largestOverlap = overlappingGenePositions
                    .MaximumItem(genePosition => GetOverlapSize(gene, genePosition));
                gene.GeneSymbol = largestOverlap.GeneSymbol;
            }

            return genes;
        }

        private double GetOverlapSize(
            GeneLocationInfo gene,
            GenePosition genePosition)
        {
            var maxStart = Math.Max(gene.StartBase, genePosition.Position.From);
            var minEnd = Math.Min(gene.EndBase, genePosition.Position.To);
            return minEnd - maxStart + 1;
        }

        private bool IsOverlapping(
            GeneLocationInfo gene,
            GenePosition genePosition)
        {
            return genePosition.Position.Overlaps(new Range<int>(gene.StartBase, gene.EndBase));
        }

        public class PersonExonVariantStatistics
        {
            public string PersonId { get; set; }
            public string GeneSymbol { get; set; }
            public string TranscriptName { get; set; }
            public int VariantCount { get; set; }
            public double? PopulationPercentile { get; set; }
        }
        public class PopulationExonVariantStatistics
        {
            public string GeneSymbol { get; set; }
            public string TranscriptName { get; set; }
            public double AverageVariantCount { get; set; }
            public double StandardDeviationVariantCount { get; set; }
            public double MedianVariantCount { get; set; }
            public double Percentile90VariantCount { get; set; }
            public List<PersonExonVariantStatistics> PersonStatistics { get; set; }
        }
    }
}
