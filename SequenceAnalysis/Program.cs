using System;
using System.IO;
using System.Linq;
using Commons;
using Domain;

namespace SequenceAnalysis
{
    public static class Program
    {
        private static readonly char[] KnownLetters = {'A', 'C', 'G', 'T', 'N' };

        private enum Mode
        {
            WholeGenome,
            Peptides
        }

        public static void Main(string[] args)
        {
            const Mode mode = Mode.Peptides;
            switch (mode)
            {
                case Mode.WholeGenome:
                    WholeGenomeFrequencyAnalysis(args);
                    break;
                case Mode.Peptides:
                    PeptideFrequencyAnalysis();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void WholeGenomeFrequencyAnalysis(string[] args)
        {
            var fileName = @"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.dna.primary_assembly.fa";
            if (args.Length > 1)
                fileName = args[0];
            var letterCount = KnownLetters.Length;
            var singleLetterFrequency = new CappedFrequencyCounter<char>(letterCount);
            var doubleLetterFrequency = new CappedFrequencyCounter<string>(letterCount * letterCount);
            var threeLetterFrequency = new CappedFrequencyCounter<string>(letterCount * letterCount * letterCount);
            var fiveLetterFrequency = new CappedFrequencyCounter<string>(letterCount * letterCount * letterCount * letterCount * letterCount);
            var tenLetterFrequency = new CappedFrequencyCounter<string>(100000);

            var letterBuffer = new CircularBuffer<char>(10);
            var baseIndex = 0uL;
            var lineIdx = 0;
            using (var streamReader = new StreamReader(fileName))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    lineIdx++;
                    if (line.StartsWith(">"))
                        continue;
                    if (lineIdx % (1000 * 1000) == 0)
                        Console.WriteLine($"Line {lineIdx}, base {baseIndex}");
                    foreach (var c in line)
                    {
                        if (!KnownLetters.Contains(c))
                            throw new Exception($"Unknown base {c}");
                        baseIndex++;
                        letterBuffer.Put(c);
                        singleLetterFrequency.Add(c);
                        if (baseIndex > 1)
                        {
                            var doubleLetter = new string(letterBuffer.Take(2).ToArray());
                            doubleLetterFrequency.Add(doubleLetter);
                        }
                        if (baseIndex > 2)
                        {
                            var threeLetter = new string(letterBuffer.Take(3).ToArray());
                            threeLetterFrequency.Add(threeLetter);
                        }
                        if (baseIndex > 4)
                        {
                            var fiveLetter = new string(letterBuffer.Take(5).ToArray());
                            fiveLetterFrequency.Add(fiveLetter);
                        }
                        if (baseIndex > 9)
                        {
                            var tenLetter = new string(letterBuffer.Take(10).ToArray());
                            tenLetterFrequency.Add(tenLetter);
                        }
                    }
                    if (lineIdx > 55 * 1000 * 1000)
                        break;
                }
            }
            File.WriteAllText("nucleotide_statistics.txt",
                singleLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + doubleLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + threeLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + fiveLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + tenLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine);
        }

        private static void PeptideFrequencyAnalysis()
        {
            var peptides = PeptideFileReader.ReadPeptides(@"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.pep.all.fa");

            var peptideCount = 25;
            var singleLetterFrequency = new FixedLengthSequenceFrequencyCounter(1, peptideCount);
            var doubleLetterFrequency = new FixedLengthSequenceFrequencyCounter(2, peptideCount*peptideCount);
            var threeLetterFrequency = new FixedLengthSequenceFrequencyCounter(3, peptideCount*peptideCount*peptideCount);
            var fiveLetterFrequency = new FixedLengthSequenceFrequencyCounter(5, 30000);
            var tenLetterFrequency = new FixedLengthSequenceFrequencyCounter(10, 50000);

            var startsingleLetterFrequency = new CappedFrequencyCounter<string>(peptideCount);
            var startdoubleLetterFrequency = new CappedFrequencyCounter<string>(peptideCount*peptideCount);
            var startthreeLetterFrequency = new CappedFrequencyCounter<string>(peptideCount*peptideCount);
            var startfiveLetterFrequency = new CappedFrequencyCounter<string>(30000);
            var starttenLetterFrequency = new CappedFrequencyCounter<string>(50000);

            var peptideIdx = 0;
            foreach (var peptide in peptides.OrderBy(x => x.Chromosome))
            {
                Console.WriteLine(peptide.GeneSymbol + ", chromosome " + peptide.Chromosome);

                for (var aminoAcidIdx = 0; aminoAcidIdx < peptide.Sequence.Count; aminoAcidIdx++)
                {
                    var aminoAcid = peptide.Sequence[aminoAcidIdx];
                    singleLetterFrequency.Add(aminoAcid);
                    doubleLetterFrequency.Add(aminoAcid);
                    threeLetterFrequency.Add(aminoAcid);
                    fiveLetterFrequency.Add(aminoAcid);
                    tenLetterFrequency.Add(aminoAcid);
                    if (aminoAcidIdx == singleLetterFrequency.SequenceLength - 1)
                    {
                        startsingleLetterFrequency.Add(singleLetterFrequency.CurrentSequence);
                    }
                    if (aminoAcidIdx == doubleLetterFrequency.SequenceLength - 1)
                    {
                        startdoubleLetterFrequency.Add(doubleLetterFrequency.CurrentSequence);
                    }
                    if (aminoAcidIdx == threeLetterFrequency.SequenceLength - 1)
                    {
                        startthreeLetterFrequency.Add(threeLetterFrequency.CurrentSequence);
                    }
                    if (aminoAcidIdx == fiveLetterFrequency.SequenceLength - 1)
                    {
                        startfiveLetterFrequency.Add(fiveLetterFrequency.CurrentSequence);
                    }
                    if (aminoAcidIdx == tenLetterFrequency.SequenceLength - 1)
                    {
                        starttenLetterFrequency.Add(tenLetterFrequency.CurrentSequence);
                    }
                }
                singleLetterFrequency.ResetSequence();
                doubleLetterFrequency.ResetSequence();
                threeLetterFrequency.ResetSequence();
                fiveLetterFrequency.ResetSequence();
                tenLetterFrequency.ResetSequence();

                peptideIdx++;
                //if(peptideIdx > 1000)
                //    break;
            }
            File.WriteAllText("peptide_statistics.txt",
                singleLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + doubleLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + threeLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + fiveLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + tenLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + "--START COMBINATIONS:---" + Environment.NewLine
                + startsingleLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + startdoubleLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + startthreeLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + startfiveLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                + "--------------------" + Environment.NewLine
                + starttenLetterFrequency.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine);
        }
    }
}
