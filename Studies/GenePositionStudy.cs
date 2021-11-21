using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;
using Commons.IO;
using Commons.Mathematics;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    public class GenePosition
    {
        public string GeneSymbol { get; }
        public string Chromosome { get; }
        public Range<int> Position { get; }

        public GenePosition(string geneSymbol, string chromosome, Range<int> position)
        {
            GeneSymbol = geneSymbol;
            Chromosome = chromosome;
            Position = position;
        }
    }

    public class GenePositionStudy
    {
        [Test]
        public void GroupGenesByPosition()
        {
            var peptidePositionFilePath = @"F:\HumanGenome\Homo_sapiens.GRCh38.pep.headers.csv";
            var genePositionOutputFilePath = @"F:\HumanGenome\gene_positions.csv";
            var overlappingGenesOutputFilePath = @"F:\HumanGenome\overlapping_genes.csv";
            var detectOverlappingGenes = true;

            var peptidePositions = CsvReader.ReadTable(peptidePositionFilePath, x => x, true, ' ');
            var entries = peptidePositions.Rows
                .Where(row => Regex.IsMatch(row["chromosome"], "^([0-9]+|X|Y|MT)$"))
                .Select(row => new GenePosition(row["gene_symbol"], row["chromosome"], new Range<int>(int.Parse(row["start"]), int.Parse(row["end"]))))
                .OrderBy(x => x.Chromosome)
                .ThenBy(x => x.Position.From);
            var mergedGenePositions = new List<GenePosition>();
            var overlappingGenesLines = new List<string> { "gene1\tgene2\tchromosome1\tstart1\tend1\tchromosome2\tstart2\tend2" };
            GenePosition currentGroup = null;
            foreach (var entry in entries)
            {
                if (currentGroup == null)
                {
                    currentGroup = entry;
                }
                else if(entry.Chromosome != currentGroup.Chromosome || !entry.Position.Overlaps(currentGroup.Position))
                {
                    mergedGenePositions.Add(currentGroup);
                    currentGroup = entry;
                }
                else if(detectOverlappingGenes && entry.GeneSymbol != currentGroup.GeneSymbol)
                {
                    Console.WriteLine("Found overlapping genes: " 
                                      + $"{currentGroup.GeneSymbol} (chr{currentGroup.Chromosome}:{currentGroup.Position.From}:{currentGroup.Position.To}) "
                                      + $"and {entry.GeneSymbol} (chr{entry.Chromosome}:{entry.Position.From}:{entry.Position.To})");
                    overlappingGenesLines.Add(string.Join("\t", new[] { 
                        currentGroup.GeneSymbol, entry.GeneSymbol, 
                        currentGroup.Chromosome, currentGroup.Position.From.ToString(), currentGroup.Position.To.ToString(),
                        entry.Chromosome, entry.Position.From.ToString(), entry.Position.To.ToString()
                    }));
                    mergedGenePositions.Add(currentGroup);
                    currentGroup = entry;
                }
                else
                {
                    var mergedPosition = new Range<int>(currentGroup.Position.From, Math.Max(currentGroup.Position.To, entry.Position.To));
                    var mergedGeneSymbol = detectOverlappingGenes ? currentGroup.GeneSymbol
                        : currentGroup.GeneSymbol.Contains(entry.GeneSymbol) ? currentGroup.GeneSymbol
                        : $"{currentGroup.GeneSymbol}|{entry.GeneSymbol}";
                    currentGroup = new GenePosition(mergedGeneSymbol, currentGroup.Chromosome, mergedPosition);
                }
            }
            if(currentGroup != null)
                mergedGenePositions.Add(currentGroup);
            File.WriteAllLines(genePositionOutputFilePath, new []{ "gene\tchromosome\tstart\tend" }
                .Concat(mergedGenePositions
                    .Select(x => $"{x.GeneSymbol}\t{x.Chromosome}\t{x.Position.From}\t{x.Position.To}")));
            if(detectOverlappingGenes)
                File.WriteAllLines(overlappingGenesOutputFilePath, overlappingGenesLines);
        }

        public static List<GenePosition> ReadGenePositions(string filePath)
        {
            using var reader = new StreamReader(filePath);
            reader.ReadLine(); // Skip header line
            var genePositions = new List<GenePosition>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var splittedLine = line.Split('\t');
                if(splittedLine.Length != 4)
                    continue;
                var genePosition = new GenePosition(splittedLine[0], splittedLine[1], new Range<int>(int.Parse(splittedLine[2]), int.Parse(splittedLine[3])));
                genePositions.Add(genePosition);
            }

            return genePositions;
        }
    }
}
