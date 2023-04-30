using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;
using GenomeTools.Tools;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    public class NucleotideAminoAcidAligner
    {
        private const int MinimumExonLength = 5;
        private const string OutputDirectory = @"C:\Temp\";// @"F:\HumanGenome\peptideAlignments\";
        private readonly GeneLocationInfoReader geneReader = new(Path.Combine(DataLocations.Root, "Homo_sapiens.GRCh38.pep.all.fa"));
        private readonly ChromosomeDataLoader chromosomeDataLoader = new(DataLocations.Chromosomes);

        [Test]
        public void TestExonExtraction()
        {
            var testPeptideSequence = "MAGVRTTGKLTT";
            var testNucleotides = NucleotideFromPeptideSequence("IIMAGVLI#ARTTIIVLRTTGKLRTT#ITT");
            var intronExtronExtractor = new IntronExonExtractor(testNucleotides, testPeptideSequence, MinimumExonLength);
            var intronExtronResult = intronExtronExtractor.Extract();
            var testAlignedSequence = BuildAlignedSequence(testNucleotides, intronExtronResult.Exons);
            File.WriteAllLines(@"C:\Temp\peptide_alignment.txt", new[]
            {
                testNucleotides,
                testAlignedSequence
            });
        }

        /// <summary>
        /// Align peptides to nucleotides (i.e. extract introns and extrons)
        /// </summary>
        [Test]
        public void AlignAllGenes()
        {
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            var peptides = geneReader.ReadAllGenes();

            string lastChromosome = null;
            string chromosomeData = null;
            foreach (var peptide in peptides.OrderBy(pep => pep.Chromosome))
            {
                var currentChromosome = peptide.Chromosome;
                if (chromosomeData == null || currentChromosome != lastChromosome)
                {
                    chromosomeData = chromosomeDataLoader.Load(currentChromosome);
                    lastChromosome = currentChromosome;
                }

                var nucleotides = chromosomeData.Substring(peptide.StartBase - 1, peptide.EndBase - peptide.StartBase + 1);
                AlignSequence(peptide, nucleotides);
            }
        }

        [Test]
        [TestCase("SNAP25", "20", 10172395, 10308258, "MAEDADMRNELEEMQRRADQLADESLESTRRMLQLVEESKDAGIRTLVMLDEQGEQLERIEEGMDQINKDMKEAEKNLTDLGKFCGLCVCPCNKLKSSDAYKKAWGNNQDGVVASQPARVVDEREQMAISGGFIRRVTNDARENEMDENLEQVSGIIGNLRHMALDMGNEIDTQNRQIDRIMEKADSNKTRIDEANQRATKMLGSG")]
        public void AlignSingleGene(string geneSymbol, string chromosome, int startBase, int endBase, string aminoAcidSequenceLetters)
        {
            var peptide = new GeneLocationInfo
            {
                GeneSymbol = geneSymbol,
                Chromosome = chromosome,
                StartBase = startBase,
                EndBase = endBase,
                AminoAcidSequence = aminoAcidSequenceLetters.Select(x => x.ToAminoAcidName()).ToList()
            };
            File.Delete(Path.Combine(OutputDirectory, peptide.GeneSymbol + ".txt"));
            var chromosomeData = chromosomeDataLoader.Load(peptide.Chromosome, startBase, endBase);
            AlignSequence(peptide, chromosomeData);
        }

        [Test]
        [TestCase(
            "SNAP25", "20", 10172395, 10308258,
            "ATCTTTGATGAGGGCAGAGCTCACGTTGCATTGAAGACGAAACCTCGGGGAGGTCAGGCGCTGTCTTTCCTTCCCTCCCTGCTCGGCGGCTCCACCACAGTTGCAACCTGCAGAGGCCCGGAGAACACAACCCTCCCGAGAAGCCCAGGTTTCTACTGAGCTGTCAGTATCTACAGAGAGATTGCTATATCATAAACAAATACATATTAAGAACCAGCCTCAGCCATCACCGAACTCAGTAAATATCTAAGACTTTGGAAGTCTTTGAGAGCAAGGGTAATATCTTAGCTTTCTTTGAATCTTCAGTACCTATAACAGTGCCTGCCACACCCATGTATGCAGGAAATAAGTGAGCGAATGGGGATACCAGCTACCCCTTTGCAGAGAAAAGAATTCTACCACTGTCCAGAGCCAAACCCGTCACTGACCCCCCAGCCCAGGCGCCCAGCCACTCCCCACCGCTACCATGGCCGAAGACGCAGACATGCGCAATGAGCTGGAGGAGATGCAGCGAAGGGCTGACCAGTTGGCTGATGAGTCGCTGGAAAGCACCCGTCGTATGCTGCAACTGGTTGAAGAGAGTAAAGATGCTGGTATCAGGACTTTGGTTATGTTGGATGAACAAGGAGAACAACTGGAACGCATTGAGGAAGGGATGGACCAAATCAATAAGGACATGAAAGAAGCAGAAAAGAATTTGACGGACCTAGGAAAATTCTGCGGGCTTTGTGTGTGTCCCTGTAACAAGCTTAAATCAAGTGATGCTTACAAAAAAGCCTGGGGCAATAATCAGGACGGAGTGGTGGCCAGCCAGCCTGCTCGTGTAGTGGACGAACGGGAGCAGATGGCCATCAGTGGCGGCTTCATCCGCAGGGTAACAAATGATGCCCGAGAAAATGAAATGGATGAAAACCTAGAGCAGGTGAGCGGCATCATCGGGAACCTCCGTCACATGGCCCTGGATATGGGCAATGAGATCGATACACAGAATCGCCAGATCGACAGGATCATGGAGAAGGCTGATTCCAACAAAACCAGAATTGATGAGGCCAACCAACGTGCAACAAAGATGCTGGGAAGTGGTTAAGTGTGCCCACCCGTGTTCTCCTCCAAATGCTGTCGGGCAAGATAGCTCCTTCATGCTTTTCTCATGGTATTATCTAGTAGGTCTGCACACATAACACACATCAGTCCACCCCCATTGTGAATGTTGTCCTGTGTCATCTGTCAGCTTCCCAACAATACTTTGTGTCTTTTGTTCTCTCTTGGTCTCTTTCTTTCCAAAGGTTGTACATAGTGGTCATTTGGTGGCTCTAACTCCTTGAGGTCTTGAGTTTCATTTTTCATTTTCTCTCCTCGGTGGCATTTGCTGAATAACAACAATTTAGGAATGCTCAATGTGCTGTTGATTCTTTCAATCCACAGTATTGTTCTTGTAAAACTGTGACATTCCACAGAGTTACTGCCACGGTCCTTTGAGTGTCAGGCTCTGAATCTCTCAAAATGTGCCGTCTTTGGTTCCTCATGGCTGTTATCTGTCTTTATGATTTCATGATTAGACAATGTGGAATTACATAACAGGCATTGCACTAAAAGTGATGTGATTTATGCATTTATGCATGAGAACTAAATAGATTTTTAGATTCCTACTTAAACAAAAACTTTCCATGACAGTAGCATACTGATGAGACAACACACACACACACAAAACAACAGCAACAACAACAGAACAACAACAAAGCATGCTCAGTATTGAGACACTGTCAAGATTAAGTTATACCAGCAAAAGTGCAGTAGTGTCACTTTTTTCCTGTCAATATATAGAGACTTCTAAATCATAATCATCCTTTTTTAAAAAAAAGAATTTTAAAAAAGATGGATTTGACACACTCACCATTTAATCATTTCCAGCAAAATATATGTTTGGCTGAAATTATGTCAAATGGATGTAATATAGGGTTTGTTTGCTGCTTTTGATGGCTATGTTTTGGAGAGAGCAATCTTGCTGTGAAACAGTGTGGATGTAAATTTTATAAGGCTGACTCTTACTAACCACCATTTCCCCTGTGGTTTGTTATCAGTACAATTCTTTGTTGCTTAATCTAGAGCTATGCACACCAAATTGCTGAGATGTTTAGTAGCTGATAAAGAAACCTTTTAAAAAAATAATATAAATGAATGAAATATAAACTGTGAGATAAATATCATTATAGCATGTAATATTAAATTCCTCCTGTCTCCTCTGTCAGTTTGTGAAGTGATTGACATTTTGTAGCTAGTTTAAAATTATTAAAAATTATAGACTCCA", 
            "MAEDADMRNELEEMQRRADQLADESLESTRRMLQLVEESKDAGIRTLVMLDEQGEQLERIEEGMDQINKDMKEAEKNLTDLGKFCGLCVCPCNKLKSSDAYKKAWGNNQDGVVASQPARVVDEREQMAISGGFIRRVTNDARENEMDENLEQVSGIIGNLRHMALDMGNEIDTQNRQIDRIMEKADSNKTRIDEANQRATKMLGSG")]
        public void AlignAminoAcidSequenceToMRNA(
            string geneSymbol,
            string chromosome,
            int startBase,
            int endBase,
            string mRNASequence,
            string aminoAcidSequence)
        {
            var peptide = new GeneLocationInfo
            {
                GeneSymbol = geneSymbol,
                Chromosome = chromosome,
                StartBase = startBase,
                EndBase = endBase,
                AminoAcidSequence = aminoAcidSequence.Select(x => x.ToAminoAcidName()).ToList()
            };
            File.Delete(Path.Combine(OutputDirectory, peptide.GeneSymbol + ".txt"));
            AlignSequence(peptide, mRNASequence);
        }

        private static void AlignSequence(
            GeneLocationInfo peptide,
            string nucleotides)
        {
            Console.WriteLine(peptide.GeneSymbol + ", chromosome " + peptide.Chromosome);
            

            var peptideDescription = $"{peptide.GeneSymbol}:{peptide.Chromosome}:{peptide.StartBase}:{peptide.EndBase}";
            try
            {
                var intronExtronExtractor = new IntronExonExtractor(nucleotides, peptide.AminoAcidSequence, MinimumExonLength);
                var intronExtronResult = intronExtronExtractor.Extract();

                var alignedSequence = BuildAlignedSequence(nucleotides, intronExtronResult.Exons);
                var statistics = new StringBuilder();
                statistics.AppendLine();
                statistics.AppendLine("Statistics:");
                statistics.AppendLine("-------------------------------");
                statistics.AppendLine($"Exon count: {intronExtronResult.Exons.Count}");
                statistics.AppendLine($"Shortest exon: {intronExtronResult.Exons.Min(e => e.AminoAcids.Count)}");
                statistics.AppendLine($"Longest exon: {intronExtronResult.Exons.Max(e => e.AminoAcids.Count)}");
                statistics.AppendLine($"Median exon length: {intronExtronResult.Exons.Median(e => e.AminoAcids.Count)}");

                File.AppendAllLines(
                    Path.Combine(OutputDirectory, peptide.GeneSymbol + ".txt"),
                    new[]
                    {
                        peptideDescription,
                        InterleaveLines(SpliceText(nucleotides, 120), SpliceText(alignedSequence, 120))
                            .Aggregate((line1, line2) => line1 + Environment.NewLine + line2),
                        statistics.ToString(),
                        Environment.NewLine
                    });
            }
            catch (Exception ex)
            {
                File.AppendAllLines(Path.Combine(OutputDirectory, peptide.GeneSymbol + ".txt"), new[] {peptideDescription, ex.Message, Environment.NewLine});
            }
        }

        /// <summary>
        /// Find genes/proteins/peptides which can be aligned nicely with the nucleotides
        /// in the chromose sequence where they are supposed to come from.
        /// Some peptides may only be small fragments that have only short exons
        /// or the exons from our own alignment are generally very short which indicates a poor alignment.
        /// These exons are not trustworthy for other experiments.
        /// </summary>
        [Test]
        public void WellAlignedSequences()
        {
            var peptides = geneReader.ReadAllGenes();

            string lastChromosome = null;
            string chromosomeData = null;
            const int LongSequenceThreshold = 10;
            foreach (var peptide in peptides.OrderBy(pep => pep.Chromosome))
            {
                Console.WriteLine(peptide.GeneSymbol + ", chromosome " + peptide.Chromosome);

                if (chromosomeData == null || peptide.Chromosome != lastChromosome)
                {
                    chromosomeData = chromosomeDataLoader.Load(peptide.Chromosome);
                    lastChromosome = peptide.Chromosome;
                }
                var nucleotides = chromosomeData.Substring(peptide.StartBase - 1, peptide.EndBase - peptide.StartBase + 1);

                var peptideDescription = $"{peptide.GeneSymbol}:{peptide.Chromosome}:{peptide.StartBase}:{peptide.EndBase}";
                try
                {
                    if (!peptide.AminoAcidSequence.Any())
                        continue;
                    if (peptide.AminoAcidSequence.First() != AminoAcidName.Methionine)
                        continue;
                    var intronExtronExtractor = new IntronExonExtractor(nucleotides, peptide.AminoAcidSequence, MinimumExonLength);
                    var intronExtronResult = intronExtronExtractor.Extract();
                    var sequenceLengths = intronExtronResult.Exons.Select(e => e.AminoAcids.Count).ToList();
                    var longSequenceMarkers = sequenceLengths.Select(x => x >= LongSequenceThreshold).ToList();
                    var hasContiguousLongSequences = Enumerable.Range(0, longSequenceMarkers.Count - 1)
                        .Select(idx => longSequenceMarkers[idx] && longSequenceMarkers[idx + 1])
                        .Any(x => x);
                    if (!hasContiguousLongSequences)
                        continue;
                    var exonData = peptideDescription + ":" + intronExtronResult.Exons
                                       .Select(e =>
                                           e.StartNucelotideIndex
                                           + ";"
                                           + e.EndNucleotideIndex)
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
            var nucleotides = new StringBuilder();
            foreach (var peptide in peptideSequence)
            {
                var codon = CodonMap.GenerateCodonForAminoAcid(peptide == '#' ? AminoAcidName.StopCodon : peptide.ToAminoAcidName());
                nucleotides.Append(codon);
            }
            return nucleotides.ToString();
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
                    exon.AminoAcids.Select(aminoAcid => $"-{ToOneLetterCode(aminoAcid.AminoAcid)}-").Aggregate((a, b) => a + b));
            }
            alignedSequence = alignedSequence.Remove(peptideNucleotides.Length);
            return alignedSequence;
        }

        private static char ToOneLetterCode(AminoAcidName aminoAcid)
        {
            switch (aminoAcid)
            {
                case AminoAcidName.StopCodon:
                    return '#';
                case AminoAcidName.Unknown:
                    return '?';
                default:
                    return aminoAcid.ToOneLetterCode();
            }
        }
    }
}
