using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.Tools
{
    /// <summary>
    /// Extracts data for training machine learning models to detect exon/intron boundaries
    /// </summary>
    public static class ExonBoundaryDataExtractor
    {
        private const string WellMatchedPeptideFile = @"F:\HumanGenome\exonData\well_matched_peptides.csv";
        private const string ChromosomeDataDirectory = @"F:\HumanGenome\chromosomes";
        private const string OutputDirectory = @"F:\HumanGenome\exonBoundaryData\";
        private const int LongSequenceThreshold = 10;
        private const int PromoterLength = 250;
        private const int DownstreamLength = 100;
        private const int ExonBoundaryLength = 25;

        public static void Extract()
        {
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            var rng = new Random();
            var peptides = ReadGenes();
            string lastChromosome = null;
            string chromosomeData = null;
            foreach (var peptide in peptides.OrderBy(pep => pep.Chromosome))
            {
                Console.WriteLine(peptide.GeneSymbol + ", chromosome " + peptide.Chromosome);
                if(!peptide.Exons.Any())
                    continue;

                var chromosomeFile = Directory.GetFiles(ChromosomeDataDirectory, $"*chromosome_{peptide.Chromosome}.*").Single();
                if (chromosomeData == null || peptide.Chromosome != lastChromosome)
                {
                    chromosomeData = File.ReadAllText(chromosomeFile);
                    lastChromosome = peptide.Chromosome;
                }
                var firstExon = peptide.Exons.First();
                var lastExon = peptide.Exons.Last();
                var promoter = chromosomeData.Substring(firstExon.StartBase - 1 - PromoterLength, PromoterLength);
                File.AppendAllText(Path.Combine(OutputDirectory, "promoter_sequences.txt"), SeparateNucelotidesByComma(promoter) + Environment.NewLine);
                var downstreamEnd = chromosomeData.Substring(lastExon.EndBase, DownstreamLength);
                File.AppendAllText(Path.Combine(OutputDirectory, "downstream_sequences.txt"), SeparateNucelotidesByComma(downstreamEnd) + Environment.NewLine);
                string negativePromoter;
                do
                {
                    var negativeStart = rng.Next(chromosomeData.Length - PromoterLength);
                    negativePromoter = chromosomeData.Substring(negativeStart, PromoterLength);
                } while (negativePromoter.Contains('N'));
                File.AppendAllText(Path.Combine(OutputDirectory, "promoter_negatives.txt"), SeparateNucelotidesByComma(negativePromoter) + Environment.NewLine);
                string negativeDownstream;
                do
                {
                    var negativeStart = rng.Next(chromosomeData.Length - DownstreamLength);
                    negativeDownstream = chromosomeData.Substring(negativeStart, DownstreamLength);
                } while (negativeDownstream.Contains('N'));
                File.AppendAllText(Path.Combine(OutputDirectory, "downstream_negatives.txt"), SeparateNucelotidesByComma(negativeDownstream) + Environment.NewLine);

                var exonLeftBoundaryData = new List<string>();
                var exonRightBoundaryData = new List<string>();
                var negativeCases = new List<string>();
                for (var exonIdx = 0; exonIdx < peptide.Exons.Count-1; exonIdx++)
                {
                    var currentExon = peptide.Exons[exonIdx];
                    var nextExon = peptide.Exons[exonIdx + 1];
                    if(!(currentExon.Length > LongSequenceThreshold) || !(nextExon.Length > LongSequenceThreshold))
                        continue;
                    var leftBoundary = chromosomeData.Substring(currentExon.EndBase-ExonBoundaryLength, 2*ExonBoundaryLength);
                    var rightBoundary = chromosomeData.Substring(nextExon.StartBase - 1 - ExonBoundaryLength, 2*ExonBoundaryLength);
                    exonLeftBoundaryData.Add(SeparateNucelotidesByComma(leftBoundary));
                    exonRightBoundaryData.Add(SeparateNucelotidesByComma(rightBoundary));
                    
                    // Negative case for training
                    string negativeCase;
                    do
                    {
                        var negativeStart = rng.Next(chromosomeData.Length - 2 * ExonBoundaryLength);
                        negativeCase = chromosomeData.Substring(negativeStart, 2 * ExonBoundaryLength);
                    } while (negativeCase.Contains('N'));
                    negativeCases.Add(SeparateNucelotidesByComma(negativeCase));
                }
                if (exonLeftBoundaryData.Any())
                {
                    File.AppendAllLines(Path.Combine(OutputDirectory, "exon_boundary_left.txt"), exonLeftBoundaryData);
                    File.AppendAllLines(Path.Combine(OutputDirectory, "exon_boundary_right.txt"), exonRightBoundaryData);
                    File.AppendAllLines(Path.Combine(OutputDirectory, "exon_boundary_negatives.txt"), negativeCases);
                    //File.AppendAllLines(Path.Combine(OutputDirectory, "exon_boundary_combined.txt"), Enumerable.Range(0,exonLeftBoundaryData.Count)
                    //    .Select(idx => exonLeftBoundaryData[idx] + exonRightBoundaryData[idx]));
                }
            }
        }

        private static string SeparateNucelotidesByComma(string promoter)
        {
            return promoter.Select(c => c + "").Aggregate((a, b) => a + "," + b);
        }

        private static List<GeneLocationInfo> ReadGenes()
        {
            var lines = File.ReadAllLines(WellMatchedPeptideFile);
            var peptides = new List<GeneLocationInfo>();
            foreach (var line in lines)
            {
                var splittedLine = line.Split(':');
                var peptide = new GeneLocationInfo
                {
                    GeneSymbol = splittedLine[0],
                    Chromosome = splittedLine[1],
                    StartBase = int.Parse(splittedLine[2]),
                    EndBase = int.Parse(splittedLine[3])
                };
                var exonBoundaries = splittedLine[4].Split(';');
                for (var boundaryIdx = 0; boundaryIdx < exonBoundaries.Length; boundaryIdx += 2)
                {
                    var startBase = peptide.StartBase + int.Parse(exonBoundaries[boundaryIdx]);
                    var endBase = peptide.StartBase + int.Parse(exonBoundaries[boundaryIdx + 1]);
                    var exon = new ExonInfo
                    {
                        StartBase = startBase,
                        EndBase = endBase
                    };
                    peptide.Exons.Add(exon);
                }
                peptides.Add(peptide);
            }
            return peptides;
        }
    }
}