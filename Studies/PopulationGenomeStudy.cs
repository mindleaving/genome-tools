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
        private const string ThousandGenomeVariantsFilePath = @"F:\datasets\mygenome\OtherGenomes\genome-1000_chr6.vcf";
        private const string GeneDatabaseName = "GenVariantStatistics";

        [Test]
        [TestCase("6")]
        public async Task ComputeGeneVariantStatistics(string chromosome)
        {
            var genePositions = GenePositionStudy.ReadGenePositions(GenePositionFilePath);

            var vcfLoader = new VcfLoader();
            var vcfHeader = vcfLoader.ReadHeader(ThousandGenomeVariantsFilePath);
            var vcfIndex = new VcfIndexReader().Read(ThousandGenomeVariantsFilePath + ".vcfi");
            var personIds = vcfHeader.Columns.Skip(9).ToList();
            var genStatisticsDb = new GeneStatisticsDb(GeneDatabaseName);
            foreach (var genePosition in genePositions.Where(x => x.Chromosome == chromosome))
            {
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
                        var parent2HasVariant = kvp.Value[2] == '1';
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

                var fileOffset = vcfIndex
                    .Where(x => x.Chromosome == genePosition.Chromosome && x.Position <= genePosition.Position.From)
                    .MaximumItem(x => x.Position)
                    .FileOffset;
                try
                {
                    vcfLoader.Load(ThousandGenomeVariantsFilePath, AnalyzeVariant, 
                        fileOffset: fileOffset, 
                        variantFilter: x => x.Chromosome == genePosition.Chromosome && genePosition.Position.Contains(x.Position),
                        stopCriteria: x => x.Chromosome != genePosition.Chromosome || x.Position > genePosition.Position.To);
                }
                catch (TaskCanceledException)
                {
                    // Ignore
                }
                foreach (var personGeneStatistic in personGeneStatistics.Values)
                {
                    personGeneStatistic.UpdateRatios();
                    await genStatisticsDb.StoreGeneStatistics(personGeneStatistic);
                }
            }
        }

        private void AddVariant(GeneVariantStatistics personStatistic, VcfVariantEntry variant, bool isHeterogenous)
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
