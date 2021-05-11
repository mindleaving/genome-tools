using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using NUnit.Framework;

namespace Studies
{
    [TestFixture]
    public partial class AlphaHelixPairStudy
    {
        [Test]
        [TestCase(@"F:\HumanGenome\helixSequences.txt", "helixPairs_n+4.txt")]
        [TestCase(@"F:\HumanGenome\nonHelixSequences.txt", "nonHelixPairs_n+4.txt")]
        [TestCase(@"F:\HumanGenome\orderedPeptides_sequenceOnly.txt", "unfilteredPairs_n+4.txt")]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\HumanProteins\helixSequences.txt", "helixPairs_n+3.txt")]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\HumanProteins\nonHelixSequences.txt", "nonHelixPairs_n+3.txt")]
        public void BuildPairStatistics(string inputFilePath, string outputFilename)
        {
            var helixSequences = File.ReadAllLines(inputFilePath)
                .Where(line => !line.StartsWith("#"))
                .ToList();
            var pairs = new Dictionary<string,int>();
            const int PairDistance = 3;
            foreach (var helixSequence in helixSequences)
            {
                for (int pairIndex = 0; pairIndex < helixSequence.Length-PairDistance; pairIndex++)
                {
                    var pair = "" + helixSequence[pairIndex] + helixSequence[pairIndex + PairDistance];
                    if(!pairs.ContainsKey(pair))
                        pairs.Add(pair, 0);
                    pairs[pair]++;
                }
            }
            var aminoAcidCodes = EnumExtensions.GetValues<AminoAcidName>().Select(x => x.ToOneLetterCode()).ToList();
            var statistics = aminoAcidCodes
                .SelectMany(aa1 => aminoAcidCodes.Select(aa2 => $"{aa1}{aa2}"))
                .Select(pair => $"{pair};{(pairs.ContainsKey(pair) ? pairs[pair] : 0)}")
                .ToList();
            File.WriteAllLines(
                Path.Combine(Path.GetDirectoryName(inputFilePath), outputFilename), 
                statistics);
        }

        [Test]
        [TestCase(@"F:\HumanGenome\helixSequences.txt", "helixHydrogenBondNeighborhood_n+3to5.txt")]
        [TestCase(@"F:\HumanGenome\nonHelixSequences.txt", "nonHelixHydrogenBondNeighborhood_n+3to5.txt")]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\HumanProteins\helixSequences.txt", "helixHydrogenBondNeighborhood_n+3to5.txt")]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\HumanProteins\nonHelixSequences.txt", "nonHelixHydrogenBondNeighborhood_n+3to5_woUnknowns.txt")]
        public void BuildHydrogenBondNeighborhoodStatistics(string inputFilePath, string outputFilename)
        {
            var helixSequences = File.ReadAllLines(inputFilePath)
                .Where(line => !line.StartsWith("#"))
                .ToList();
            var pairs = new Dictionary<string,int>();
            const int PairDistance = 4;
            const int NeighborhoodSize = 1;
            foreach (var helixSequence in helixSequences)
            {
                for (int pairIndex = 0; pairIndex < helixSequence.Length-PairDistance-NeighborhoodSize; pairIndex++)
                {
                    var pair = $"{helixSequence[pairIndex]}-{helixSequence.Substring(pairIndex + PairDistance - NeighborhoodSize, 2 * NeighborhoodSize + 1)}";
                    if(pair.Contains("?"))
                        continue;
                    if(!pairs.ContainsKey(pair))
                        pairs.Add(pair, 0);
                    pairs[pair]++;
                }
            }
            var statistics = pairs
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key + ";" + kvp.Value)
                .ToList();
            File.WriteAllLines(
                Path.Combine(Path.GetDirectoryName(inputFilePath), outputFilename), 
                statistics);
        }

        [Test]
        [TestCase(@"F:\HumanGenome\fullPdbSequencesHelixMarked.txt", "nonHelixSequences.txt")]
        [TestCase(@"F:\HumanGenome\Protein-PDBs\HumanProteins\humanFullyPositionedSingleChainUniqueProteinHelixMarked.txt", "nonHelixSequences.txt")]
        public void ExtractNonHelixSequences(string annotatedSequencesFile, string outputFilename)
        {
            var annotatedSequences = AlphaHelixStrengthStudy.ParseHelixSequences(annotatedSequencesFile);
            var lines = new List<string>();
            foreach (var annotatedSequence in annotatedSequences)
            {
                lines.Add("#" + annotatedSequence.PdbCode);
                var currentSequence = "";
                for (int i = 0; i < annotatedSequence.AminoAcidCodes.Count; i++)
                {
                    var isHelix = annotatedSequence.IsHelixSignal[i];
                    var aminoAcidCode = annotatedSequence.AminoAcidCodes[i];
                    if (isHelix)
                    {
                        if (currentSequence.Length > 0)
                        {
                            lines.Add(currentSequence);
                            currentSequence = "";
                        }
                    }
                    else
                    {
                        currentSequence += aminoAcidCode;
                    }
                }
                if (currentSequence.Length > 0)
                {
                    lines.Add(currentSequence);
                }
            }
            File.WriteAllLines(
                Path.Combine(Path.GetDirectoryName(annotatedSequencesFile), outputFilename),
                lines);
        }

        [Test]
        [TestCase(@"F:\HumanGenome\helixSequences.txt", "helixNeighborPairs_n+4_offset3.txt")]
        [TestCase(@"F:\HumanGenome\nonHelixSequences.txt", "nonHelixNeigborPairs_n+4_offset3.txt")]
        public void HelixNeighborPairs(string inputFilePath, string outputFilename)
        {
            var helixSequences = File.ReadAllLines(inputFilePath)
                .Where(line => !line.StartsWith("#"))
                .ToList();
            var pairs = new Dictionary<string,int>();
            const int PairDistance = 4;
            const int PairOffset = 3;
            foreach (var helixSequence in helixSequences)
            {
                for (int pairIndex = 0; pairIndex < helixSequence.Length-PairDistance-PairOffset; pairIndex++)
                {
                    var pair1 = "" + helixSequence[pairIndex] + helixSequence[pairIndex + PairDistance];
                    var pair2 = "" + helixSequence[pairIndex + PairOffset] + helixSequence[pairIndex + PairDistance + PairOffset];
                    var combinedPair = $"{pair1}-{pair2}";
                    if(!pairs.ContainsKey(combinedPair))
                        pairs.Add(combinedPair, 0);
                    pairs[combinedPair]++;
                }
            }
            var statistics = pairs
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key + ";" + kvp.Value)
                .ToList();
            File.WriteAllLines(
                Path.Combine(Path.GetDirectoryName(inputFilePath), outputFilename), 
                statistics);
        }
    }
}
