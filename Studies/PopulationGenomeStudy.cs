using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using GenomeTools.Studies.GenomeAnalysis;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    /// <summary>
    /// Analyze data from population genome projects like the 1000 genomes project
    /// </summary>
    public class PopulationGenomeStudy
    {
        private const string GenePositionFilePath = @"F:\HumanGenome\gene_positions.csv";
        private const string ThousandGenomeVariantsFilePath = @"F:\datasets\mygenome\OtherGenomes\genome-1000_chr$$.vcf";
        private const string GeneDatabaseName = "GenVariantStatistics";

        [Test]
        public async Task ComputeAggregatedPopulationVariantStatistics()
        {
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            var genStatisticsDb = new GeneVariantDb(GeneDatabaseName);
            foreach (var genePosition in genePositions)
            {
                var genVariantStatistics = await genStatisticsDb.GetGeneVariantStatistics(genePosition.GeneSymbol);
                var filteredVariantStatistics = genVariantStatistics.Where(x => x.PersonId != WholeGenomeSequencingStudy.PersonId).ToList();
                var aggregatedStatistics = new AggregatedGeneVariantStatistics(filteredVariantStatistics);
                await genStatisticsDb.Store(aggregatedStatistics);
            }
        }

        [Test]
        [TestCase("4")]
        [TestCase("3")]
        [TestCase("2")]
        [TestCase("1")]
        public async Task ComputeGeneVariantStatistics(string chromosome)
        {
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);
            Console.WriteLine("Gene positions loaded");

            var variantFilePath = ThousandGenomeVariantsFilePath.Replace("$$",chromosome);
            var vcfAccessor = new ParallelizedVcfAccessor(null, variantFilePath);
            var vcfHeader = vcfAccessor.ReadHeader();
            var personIds = vcfHeader.Columns.Skip(9).ToList();
            var genStatisticsDb = new GeneVariantDb(GeneDatabaseName);
            foreach (var genePosition in genePositions.Where(x => x.Chromosome == chromosome))
            {
                Console.Write($"Processing variants for gene {genePosition.GeneSymbol}...");
                var personGeneStatistics = new Dictionary<string, GeneVariantStatistics>();
                foreach (var personId in personIds)
                {
                    personGeneStatistics[$"{personId}_P1"] = new GeneVariantStatistics(personId, GeneParentalOrigin.Parent1, genePosition);
                    personGeneStatistics[$"{personId}_P2"] = new GeneVariantStatistics(personId, GeneParentalOrigin.Parent2, genePosition);
                }
                void AnalyzeVariant(VcfVariantEntry variant, List<VcfMetadataEntry> metadata)
                {
                    foreach (var kvp in variant.OtherFields.Skip(1)) // Skip 1 for FORMAT column
                    {
                        var personId = kvp.Key;
                        if(kvp.Value == "0|0")
                            continue;
                        var parent1HasVariant = kvp.Value[0] == '1';
                        var parent2HasVariant = kvp.Value.Length == 3 && kvp.Value[2] == '1';
                        var isHeterogenous = parent1HasVariant != parent2HasVariant;
                        if (parent1HasVariant)
                        {
                            var personStatistic = personGeneStatistics[$"{personId}_P1"];
                            AddVariant(personStatistic, variant, isHeterogenous);
                        }
                        if (parent2HasVariant)
                        {
                            var personStatistic = personGeneStatistics[$"{personId}_P2"];
                            AddVariant(personStatistic, variant, isHeterogenous);
                        }
                    }
                }
                await vcfAccessor.LoadInRange(genePosition, AnalyzeVariant);
                foreach (var personGeneStatistic in personGeneStatistics.Values)
                {
                    personGeneStatistic.UpdateRatios();
                    await genStatisticsDb.Store(personGeneStatistic);
                }
                Console.WriteLine("DONE");
            }
        }

        public static void AddVariant(GeneVariantStatistics personStatistic, VcfVariantEntry variant, bool isHeterogenous)
        {
            var alternateBases = variant.AlternateBases.Last().Replace(".","").Replace("*", "");
            if(Regex.IsMatch(alternateBases, "^<.*>$"))
                return;
            personStatistic.VariantCount++;
            personStatistic.VariantPositions.Add(variant.Position);
            if (isHeterogenous)
                personStatistic.HeterogenousCount++;
            var lengthDifference = Math.Abs(alternateBases.Length - variant.ReferenceBases.Length);
            var isDeletion = alternateBases.Length < variant.ReferenceBases.Length;
            if(isDeletion)
            {
                personStatistic.DeletionCount++;
                personStatistic.DeletionLength += lengthDifference;
                personStatistic.TotalVariantLength += lengthDifference;
                return;
            }
            var isInsert = alternateBases.Length > variant.ReferenceBases.Length;
            if (isInsert)
            {
                personStatistic.InsertionCount++;
                personStatistic.InsertionLength += lengthDifference;
                personStatistic.TotalVariantLength += lengthDifference;
                return;
            }
            var isSNP = alternateBases.Length == 1 && variant.ReferenceBases.Length == 1;
            if (isSNP)
            {
                personStatistic.SNPCount++;
                personStatistic.TotalVariantLength += 1;
                return;
            }
        }
    }
}
