using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;
using GenomeTools.Tools;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    public class IntronFoldingStudy
    {
        private readonly GeneLocationInfoReader geneReader = new(Path.Combine(DataLocations.Root, "Homo_sapiens.GRCh38.pep.all.fa"));
        private readonly ChromosomeDataLoader chromosomeDataLoader = new(DataLocations.Chromosomes);

        [Test]
        public void ExtractLMNAIntron()
        {
            var genes = geneReader.ReadGenesForSymbol("LMNA");
            var gene = genes.First(x => x.AminoAcidSequence.ToStringOfOneLetterCodes().EndsWith("CSIM"));
            ExtractIntron(gene);
        }

        private void ExtractIntron(GeneLocationInfo gene)
        {
            var aminoAcidSequence = gene.AminoAcidSequence;
            var nucleotides = chromosomeDataLoader.Load(gene);

            var intronExtronExtractor = new IntronExonExtractor(nucleotides, aminoAcidSequence, 5);
            var result = intronExtronExtractor.Extract();
            Console.WriteLine($"Exons: {result.Exons.Count}");
            Console.WriteLine($"Introns: {result.Introns.Count}");

            // Base pairing and folding
            //var intronFolder = new IntronFolder();
            //for (var intronIndex = 0; intronIndex < result.Introns.Count; intronIndex++)
            //{
            //    var intron = result.Introns[intronIndex];
            //    var foldedIntron = intronFolder.Fold(intron);
            //    var lines = new []{ $"{intron.StartNucelotideIndex};{intron.StartNucelotideIndex + intron.Nucelotides.Count - 1}" }
            //        .Concat(foldedIntron.BasePairings.Select(pair => $"{pair.Base1Index};{pair.Base2Index}"));
            //    File.WriteAllLines(
            //        $@"D:\HumanGenome\intronBasePairings\basePairings_{gene.GeneSymbol}_intron{intronIndex:00}.csv",
            //        lines);
            //}

            // Intron convolution
            for (var intronIndex = 0; intronIndex < result.Introns.Count; intronIndex++)
            {
                var intron = result.Introns[intronIndex];
                var sequence = intron.Nucelotides.Take(30).ToList();
                var reverseSequence = sequence.AsEnumerable().Reverse().ToList();
                var matchStatistics = new List<string>();
                for (int offset = -sequence.Count+1; offset < sequence.Count; offset++)
                {
                    var overlapSequence1 = sequence.Skip(-offset).Take(sequence.Count - offset).ToList();
                    var overlapSequence2 = reverseSequence.Skip(offset).Take(reverseSequence.Count + offset).ToList();
                    var matchCount = overlapSequence1.PairwiseOperation(
                            overlapSequence2,
                            (n1, n2) => NucleotideExtensions.IsComplementaryMatch(n1, n2) ? 1 : 0
                        )
                        .Sum();
                    var matchRatio = matchCount / (double) overlapSequence1.Count;
                    matchStatistics.Add($"{offset};{matchRatio:F4}");
                }

                File.WriteAllLines(
                    $@"D:\HumanGenome\intronBasePairings\matchRatio_{gene.GeneSymbol}_intron{intronIndex:00}.csv",
                    matchStatistics);
            }
        }
    }
}
