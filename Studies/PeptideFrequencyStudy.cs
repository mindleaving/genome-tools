using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.DataProcessing;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;
using GenomeTools.Tools;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    [TestFixture]
    public class PeptideFrequencyStudy
    {
        private readonly GeneLocationInfoReader geneReader = new(Path.Combine(DataLocations.Root, "Homo_sapiens.GRCh38.pep.all.fa"));

        [Test]
        public void FindCommonPeptideCombinations()
        {
            var outputDirectory = @"F:\HumanGenome\sequenceFrequencies\AllGRCh38Peptides";
            var peptides = geneReader.ReadAllGenes();
            Analyze(peptides, outputDirectory);
        }

        public static void Analyze(List<GeneLocationInfo> peptides, string outputDirectory)
        {
            var peptideCount = 25;
            var singleLetterFrequency = new FixedLengthSequenceFrequencyCounter(1, peptideCount);
            var doubleLetterFrequency = new FixedLengthSequenceFrequencyCounter(2, peptideCount*peptideCount);
            var threeLetterFrequency = new FixedLengthSequenceFrequencyCounter(3, peptideCount*peptideCount*peptideCount);
            var fiveLetterFrequency = new FixedLengthSequenceFrequencyCounter(5, 30000);
            var tenLetterFrequency = new FixedLengthSequenceFrequencyCounter(10, 50000);
            var thirtyLetterFrequency = new FixedLengthSequenceFrequencyCounter(30, 50000);

            var startsingleLetterFrequency = new CappedFrequencyCounter<string>(peptideCount);
            var startdoubleLetterFrequency = new CappedFrequencyCounter<string>(peptideCount*peptideCount);
            var startthreeLetterFrequency = new CappedFrequencyCounter<string>(peptideCount*peptideCount);
            var startfiveLetterFrequency = new CappedFrequencyCounter<string>(30000);
            var starttenLetterFrequency = new CappedFrequencyCounter<string>(50000);

            var peptideIdx = 0;
            foreach (var peptide in peptides.OrderBy(x => x.Chromosome, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine(peptide.GeneSymbol + ", chromosome " + peptide.Chromosome);

                for (var aminoAcidIdx = 0; aminoAcidIdx < peptide.AminoAcidSequence.Count; aminoAcidIdx++)
                {
                    var aminoAcid = peptide.AminoAcidSequence[aminoAcidIdx].ToOneLetterCode();
                    singleLetterFrequency.Add(aminoAcid);
                    doubleLetterFrequency.Add(aminoAcid);
                    threeLetterFrequency.Add(aminoAcid);
                    fiveLetterFrequency.Add(aminoAcid);
                    tenLetterFrequency.Add(aminoAcid);
                    thirtyLetterFrequency.Add(aminoAcid);
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
                thirtyLetterFrequency.ResetSequence();

                peptideIdx++;
                //if(peptideIdx > 1000)
                //    break;
            }
            File.WriteAllText(Path.Combine(outputDirectory, "peptide_statistics.txt"), ""
                 + singleLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                 + "--------------------" + Environment.NewLine
                 + doubleLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                 + "--------------------" + Environment.NewLine
                 + threeLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                 + "--------------------" + Environment.NewLine
                 + fiveLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                 + "--------------------" + Environment.NewLine
                 + tenLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
                 + "--------------------" + Environment.NewLine
                 + thirtyLetterFrequency.FrequencyCounter.Counter.OrderByDescending(kvp => kvp.Value).Take(1000).Select(kvp => kvp.Key + ": " + kvp.Value).Aggregate((a, b) => a + Environment.NewLine + b) + Environment.NewLine
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
