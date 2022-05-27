using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private const string ExonPositionFilePath = @"F:\HumanGenome\exonBoundaryData\exonPositions_ucsc-refseq";
        private const string GenePositionFilePath = @"F:\HumanGenome\gene_positions.csv";
        private const string Vcf1000GenomesFilePath = @"F:\datasets\mygenome\OtherGenomes\genome-1000_%CHROMOSOME%.vcf";

        [Test]
        public void ExtractVariantsOverlappingExonsForMyGenome()
        {
            var outputFilePath = @"F:\datasets\mygenome\variants_in_exons.csv";
            var vcfLoader = new VcfAccessor(PersonId, VariantFilePath);
            var genesWithExons = LoadExons(ExonPositionFilePath);
            var chromosomeGenes = genesWithExons.Values.GroupBy(gene => gene.Chromosome).ToDictionary(g => g.Key, g => g.ToList());
            var affectedGenes = new Dictionary<string,GeneExonVariantStatistics>();
            var output = new List<string> { "gene_symbol;transcript;chromosome;variant_position;is_heterogenous;reference;alternate" };
            var exonVariants = new List<ExonVariant>();
            void AnalyzeVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadata)
            {
                if(!chromosomeGenes.ContainsKey(variant.Chromosome))
                    return;
                var relevantGenes = chromosomeGenes[variant.Chromosome];
                var overlappingGenes = relevantGenes.Where(gene => IsOverlapping(gene, variant));
                var genomeId = variant.GetGenomeIds().Single();
                foreach (var gene in overlappingGenes)
                {
                    var overlappingExon = gene.Exons.FirstOrDefault(exon => IsOverlapping(exon, variant));
                    if (overlappingExon == null) 
                        continue;
                    if(!affectedGenes.ContainsKey(gene.TranscriptName))
                    {
                        affectedGenes.Add(gene.TranscriptName, new GeneExonVariantStatistics
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
            vcfLoader.Load(AnalyzeVariant);
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
            File.WriteAllLines(outputFilePath, output);

            var allGeneSymbols = affectedGenes.Values
                .Select(gene => gene.GeneSymbol)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            output.Clear();
            foreach (var geneSymbol in allGeneSymbols)
            {
                var allTranscriptNames = affectedGenes.Values
                    .Where(gene => gene.GeneSymbol == geneSymbol)
                    .Select(gene => gene.TranscriptName)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                foreach (var transcriptName in allTranscriptNames)
                {
                    var variantCount = affectedGenes[transcriptName].VariantCount;
                    output.Add($"{geneSymbol};{transcriptName};{variantCount}");
                }
            }
            outputFilePath = @"F:\datasets\mygenome\variants_in_exons_statistics.csv";
            File.WriteAllLines(outputFilePath, output);
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
            var genesWithExons = LoadExons(ExonPositionFilePath);
            var chromosomeGenes = genesWithExons.Values.Where(gene => gene.Chromosome == chromosome).ToList();
            var personExonVariantStatistics = new Dictionary<string, GeneExonVariantStatistics>();

            var variantCount = 0;
            void AnalyzeVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadata)
            {
                var overlappingGenes = chromosomeGenes.Where(gene => IsOverlapping(gene, variant));
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
                            personExonVariantStatistics.Add(statisticsKey, new GeneExonVariantStatistics
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

            var personIds = personExonVariantStatistics.Values.Select(gene => gene.PersonId).Distinct().ToList();
            var output = new List<string>
            {
                $"gene_symbol;transcript;average;median;{string.Join(";", personIds)}"
            };
            var allGeneSymbols = personExonVariantStatistics.Values
                .Select(gene => gene.GeneSymbol)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            foreach (var geneSymbol in allGeneSymbols)
            {
                var allTranscriptNames = personExonVariantStatistics.Values
                    .Where(gene => gene.GeneSymbol == geneSymbol)
                    .Select(gene => gene.TranscriptName)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                foreach (var transcriptName in allTranscriptNames)
                {
                    var transcriptStatistics = personExonVariantStatistics.Values
                        .Where(x => x.GeneSymbol == geneSymbol && x.TranscriptName == transcriptName)
                        .ToDictionary(gene => gene.PersonId);
                    var personStatistics = personIds
                        .Select(personId => 
                            transcriptStatistics.ContainsKey(personId) 
                            ? transcriptStatistics[personId].VariantCount 
                            : 0
                        )
                        .ToList();
                    var averageVariantCount = personStatistics.Average();
                    var medianVariantCount = personStatistics.Select(x => (double)x).Median();
                    output.Add($"{geneSymbol};{transcriptName};{averageVariantCount:F2};{medianVariantCount:F1};{string.Join(";", personStatistics)}");
                }
            }
            File.WriteAllLines(outputFilePath, output);
        }

        private bool IsOverlapping(
            ExonInfo exon,
            VcfVariantEntry variant)
        {
            if (exon.EndBase < variant.Position)
                return false;
            if (exon.StartBase > variant.Position + variant.ReferenceBases.Length)
                return false;
            return true;
        }

        private bool IsOverlapping(
            GeneLocationInfo gene,
            VcfVariantEntry variant)
        {
            if (gene.EndBase < variant.Position)
                return false;
            if (gene.StartBase > variant.Position + variant.ReferenceBases.Length)
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
            return minEnd - maxStart;
        }

        private bool IsOverlapping(
            GeneLocationInfo gene,
            GenePosition genePosition)
        {
            return genePosition.Position.Overlaps(new Range<int>(gene.StartBase, gene.EndBase));
        }

        public class GeneExonVariantStatistics
        {
            public string PersonId { get; set; }
            public string GeneSymbol { get; set; }
            public string TranscriptName { get; set; }
            public int VariantCount { get; set; }
        }
    }
}
