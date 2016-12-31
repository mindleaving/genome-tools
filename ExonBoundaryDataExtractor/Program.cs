using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExonBoundaryDataExtractor
{
    public static class Program
    {
        private const string WellMatchedPeptideFile = @"G:\Projects\HumanGenome\exonData\well_matched_peptides.csv";
        private const string ChromosomeDataDirectory = @"G:\Projects\HumanGenome\chromosomes";
        private const string OutputDirectory = @"G:\Projects\HumanGenome\exonBoundaryData\";
        private const int LongSequenceThreshold = 10;
        private const int PromoterLength = 250;
        private const int DownstreamLength = 100;
        private const int ExonBoundaryLength = 11;

        public static void Main()
        {
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            var peptides = ReadPeptides();
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
                File.AppendAllText(Path.Combine(OutputDirectory, "promoter_sequences.txt"), promoter + Environment.NewLine);
                var downstreamEnd = chromosomeData.Substring(lastExon.EndBase, DownstreamLength);
                File.AppendAllText(Path.Combine(OutputDirectory, "downstream_sequences.txt"), downstreamEnd + Environment.NewLine);

                var exonLeftBoundaryData = new List<string>();
                var exonRightBoundaryData = new List<string>();
                for (var exonIdx = 0; exonIdx < peptide.Exons.Count-1; exonIdx++)
                {
                    var currentExon = peptide.Exons[exonIdx];
                    var nextExon = peptide.Exons[exonIdx + 1];
                    if(!currentExon.LongSequence || !nextExon.LongSequence)
                        continue;
                    var leftBoundary = chromosomeData.Substring(currentExon.EndBase, ExonBoundaryLength);
                    var rightBoundary = chromosomeData.Substring(nextExon.StartBase - 1 - ExonBoundaryLength, ExonBoundaryLength);
                    exonLeftBoundaryData.Add(leftBoundary);
                    exonRightBoundaryData.Add(rightBoundary);
                }
                if (exonLeftBoundaryData.Any())
                {
                    File.AppendAllLines(Path.Combine(OutputDirectory, "exon_boundary_left.txt"), exonLeftBoundaryData);
                    File.AppendAllLines(Path.Combine(OutputDirectory, "exon_boundary_right.txt"), exonRightBoundaryData);
                    File.AppendAllLines(Path.Combine(OutputDirectory, "exon_boundary_combined.txt"), Enumerable.Range(0,exonLeftBoundaryData.Count)
                        .Select(idx => exonLeftBoundaryData[idx] + exonRightBoundaryData[idx]));
                }
            }
        }

        private static List<Peptide> ReadPeptides()
        {
            var lines = File.ReadAllLines(WellMatchedPeptideFile);
            var peptides = new List<Peptide>();
            foreach (var line in lines)
            {
                var splittedLine = line.Split(':');
                var peptide = new Peptide
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
                    var exon = new Exon
                    {
                        StartBase = startBase,
                        EndBase = endBase,
                        LongSequence = endBase-startBase+1 > LongSequenceThreshold
                    };
                    peptide.Exons.Add(exon);
                }
                peptides.Add(peptide);
            }
            return peptides;
        }
    }

    public class Peptide
    {
        public string Chromosome { get; set; }
        public int StartBase { get; set; }
        public int EndBase { get; set; }
        public string GeneSymbol { get; set; }
        public List<Exon> Exons { get; } = new List<Exon>();
    }

    public class Exon
    {
        public int StartBase { get; set; }
        public int EndBase { get; set; }
        public bool LongSequence { get; set; }
    }
}
