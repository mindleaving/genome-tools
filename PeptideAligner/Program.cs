using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Commons;
using Commons.Extensions;
using Domain;

namespace PeptideAligner
{
    public static class Program
    {
        private const string ChromosomeDataDirectory = @"G:\Projects\HumanGenome\chromosomes";
        private const string OutputDirectory = @"G:\Projects\HumanGenome\peptideAlignments\";

        private enum Mode
        {
            Test,
            SequenceAlignment,
            WellMatchedPeptides
        }
        public static void Main()
        {
            var mode = Mode.WellMatchedPeptides;
            switch (mode)
            {
                case Mode.Test:
                    TestExonExtraction();
                    break;
                case Mode.SequenceAlignment:
                    SequenceAlignment();
                    break;
                case Mode.WellMatchedPeptides:
                    WellAlignedSequences();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void TestExonExtraction()
        {
            var testPeptideSequence = "MAGVRTTGKLTT";
            var testNucleotides = NucleotideFromPeptideSequence("IIMAGVLI#ARTTIIVLRTTGKLRTT#ITT");
            var testExons = ExonExtractor.ExtractExons(testNucleotides, testPeptideSequence);
            var testAlignedSequence = BuildAlignedSequence(testNucleotides, testExons);
            File.WriteAllLines(@"peptide_alignment.txt", new[]
            {
                testNucleotides,
                testAlignedSequence
            });
        }

        private static void SequenceAlignment()
        {
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            var peptides = PeptideFileReader.ReadPeptides(@"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.pep.all.fa");

            string lastChromosome = null;
            string chromosomeData = null;
            foreach (var peptide in peptides.OrderBy(pep => pep.Chromosome))
            {
                Console.WriteLine(peptide.GeneSymbol + ", chromosome " + peptide.Chromosome);

                var chromosomeFile = Directory.GetFiles(ChromosomeDataDirectory, $"*chromosome_{peptide.Chromosome}.*").Single();
                if (chromosomeData == null || peptide.Chromosome != lastChromosome)
                {
                    chromosomeData = File.ReadAllText(chromosomeFile);
                    lastChromosome = peptide.Chromosome;
                }
                var nucleotides = chromosomeData.Substring(peptide.StartBase - 1, peptide.EndBase - peptide.StartBase + 1);

                var peptideDescription = $"{peptide.GeneSymbol}:{peptide.Chromosome}:{peptide.StartBase}:{peptide.EndBase}";
                try
                {
                    var exons = ExonExtractor.ExtractExons(nucleotides, new string(peptide.Sequence.ToArray()));

                    var alignedSequence = BuildAlignedSequence(nucleotides, exons);
                    var statistics = Environment.NewLine
                                     + "Statistics:" + Environment.NewLine
                                     + "-------------------------------" + Environment.NewLine
                                     + "Exon count: " + exons.Count + Environment.NewLine
                                     + "Shortest exon: " + exons.Min(e => e.AminoAcids.Count) + Environment.NewLine
                                     + "Longest exon: " + exons.Max(e => e.AminoAcids.Count) + Environment.NewLine
                                     + "Median exon length: " + exons.Median(e => e.AminoAcids.Count);
                    File.AppendAllLines(Path.Combine(OutputDirectory, peptide.GeneSymbol + ".txt"), new[]
                    {
                        peptideDescription,
                        InterleaveLines(SpliceText(nucleotides, 120),SpliceText(alignedSequence, 120))
                            .Aggregate((line1, line2) => line1 + Environment.NewLine + line2),
                        statistics,
                        Environment.NewLine
                    });
                }
                catch (Exception ex)
                {
                    File.AppendAllLines(Path.Combine(OutputDirectory, peptide.GeneSymbol + ".txt"), new[]
                        {
                            peptideDescription,
                            ex.Message,
                            Environment.NewLine
                        });
                }
            }
        }

        private static void WellAlignedSequences()
        {
            var peptides = PeptideFileReader.ReadPeptides(@"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.pep.all.fa");

            string lastChromosome = null;
            string chromosomeData = null;
            const int longSequenceThreshold = 10;
            foreach (var peptide in peptides.OrderBy(pep => pep.Chromosome))
            {
                Console.WriteLine(peptide.GeneSymbol + ", chromosome " + peptide.Chromosome);

                var chromosomeFile = Directory.GetFiles(ChromosomeDataDirectory, $"*chromosome_{peptide.Chromosome}.*").Single();
                if (chromosomeData == null || peptide.Chromosome != lastChromosome)
                {
                    chromosomeData = File.ReadAllText(chromosomeFile);
                    lastChromosome = peptide.Chromosome;
                }
                var nucleotides = chromosomeData.Substring(peptide.StartBase - 1, peptide.EndBase - peptide.StartBase + 1);

                var peptideDescription = $"{peptide.GeneSymbol}:{peptide.Chromosome}:{peptide.StartBase}:{peptide.EndBase}";
                try
                {
                    if (!peptide.Sequence.Any())
                        continue;
                    if (peptide.Sequence.First() != 'M')
                        continue;
                    var exons = ExonExtractor.ExtractExons(nucleotides, new string(peptide.Sequence.ToArray()));
                    var sequenceLengths = exons.Select(e => e.AminoAcids.Count).ToList();
                    var longSequenceMarkers = sequenceLengths.Select(x => x >= longSequenceThreshold).ToList();
                    var hasContiguousLongSequences = Enumerable.Range(0, longSequenceMarkers.Count - 1)
                        .Select(idx => longSequenceMarkers[idx] && longSequenceMarkers[idx + 1])
                        .Any(x => x);
                    if (!hasContiguousLongSequences)
                        continue;
                    var exonData = peptideDescription + ":" + exons
                                       .Select(e =>
                                           e.StartNucelotideIndex
                                           + ";"
                                           + (e.StartNucelotideIndex + 3 * e.AminoAcids.Count - 1))
                                       .Aggregate((a, b) => a + ";" + b);
                    File.AppendAllText(@"well_matched_peptides.csv", exonData + Environment.NewLine);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static string NucleotideFromPeptideSequence(string peptideSequence)
        {
            var nucleotides = "";
            var inverseLookup = NucleotideSequenceParser.CodonLookup.ToLookup(x => x.Value, x => x.Key);
            foreach (var peptide in peptideSequence)
            {
                nucleotides += inverseLookup[peptide].First();
            }
            return nucleotides;
        }

        private static IList<string> InterleaveLines(IList<string> lines1, IList<string> lines2)
        {
            var maxLines = Math.Max(lines1.Count, lines2.Count);
            var interleavedLines = new List<string>();
            for (int linePairIdx = 0; linePairIdx < maxLines; linePairIdx++)
            {
                interleavedLines.Add(lines1.Count > linePairIdx ? lines1[linePairIdx] : "");
                interleavedLines.Add(lines2.Count > linePairIdx ? lines2[linePairIdx] : "");
            }
            return interleavedLines;
        }

        private static IList<string> SpliceText(string text, int lineLength)
        {
            return Regex.Matches(text, ".{1," + lineLength + "}").Cast<Match>().Select(m => m.Value).ToArray();
        }

        private static string BuildAlignedSequence(string peptideNucleotides, List<Exon> exons)
        {
            var alignedSequence = new string(' ', peptideNucleotides.Length);
            foreach (var exon in exons) // Must be ordered by start index
            {
                alignedSequence = alignedSequence.Insert(exon.StartNucelotideIndex,
                    exon.AminoAcids.Select(aminoAcid => $"-{aminoAcid}-").Aggregate((a, b) => a + b));
            }
            alignedSequence = alignedSequence.Remove(peptideNucleotides.Length);
            return alignedSequence;
        }
    }
}
