using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ChemistryLibrary;
using Commons;
using NUnit.Framework;
using Commons.Optimization;

namespace Studies
{
    [TestFixture]
    public class AlphaHelixStrengthStudy
    {
        /// <summary>
        /// Takes peptide sequences and optimizes amino acid-amino acid bonds
        /// such that amino acids known to be in helices are marked as helices
        /// and those who don't, don't.
        /// The two neighboring amino acids and those two amino acids at distance 4 are taken into account.
        /// </summary>
        [Test]
        public void EstimateAminoAcidHelixAffinity()
        {
            var annotatedSequencesFile = @"G:\Projects\HumanGenome\fullPdbSequencesHelixMarked.txt";
            var annotatedSequences = ParseHelixSequences(annotatedSequencesFile);
            //var aminoAcidPairs = GetAminoAcidPairs(annotatedSequences);
            //var leastCommonPair = aminoAcidPairs.OrderBy(kvp => kvp.Value).First();

            Func<double[], double> costFunc = parameters => HelixSequenceCostFunc(parameters, annotatedSequences);
            var parameterSettings = GeneratePairwiseAminoAcidParameters();

            var randomizedStartValueIterations = 100;
            //Parallel.For(0L, randomizedStartValueIterations, idx =>
            for (int idx = 0; idx < randomizedStartValueIterations; idx++)
            {
                RandomizeStartValues(parameterSettings, 2);
                var optimizationResult = GradientDescentOptimizer.Optimize(costFunc, parameterSettings, double.NegativeInfinity);
                WriteOptimizationResult(@"G:\Projects\HumanGenome\helixAffinityOptimizationResults.dat", optimizationResult);
            }
        }

        [Test]
        public void OutputClassification()
        {
            var annotatedSequencesFile = @"G:\Projects\HumanGenome\fullPdbSequencesHelixMarked.txt";
            var annotatedSequences = ParseHelixSequences(annotatedSequencesFile);

            GeneratePairwiseAminoAcidParameters();
            var bestOptimizationResult = File.ReadAllLines(@"G:\Projects\HumanGenome\helixAffinityOptimizationResults.dat")
                    .Where(line => line.Split(';').Length > 2)
                    .OrderBy(line => double.Parse(line.Split(';')[0], CultureInfo.InvariantCulture))
                    .First();
            var parameters = bestOptimizationResult.Split(';').Skip(2).Select(double.Parse).ToArray();
            var directNeighborInfluence = parameters[0];
            var fourthNeighborInfluence = parameters[1];
            var bondAffinityLookup = CreateBondAffinityLookup(parameters);
            var classifiedSequences = new List<string>();
            foreach (var annotatedSequence in annotatedSequences)
            {
                classifiedSequences.Add(GenerateAnnotatedSequence(annotatedSequence.AminoAcidCodes, annotatedSequence.IsHelixSignal));
                var classificationResult = ClassifySequence(annotatedSequence.AminoAcidCodes, bondAffinityLookup, directNeighborInfluence, fourthNeighborInfluence);
                var classifiedSequence = GenerateAnnotatedSequence(annotatedSequence.AminoAcidCodes, classificationResult.Select(x => x > 0).ToList());
                classifiedSequences.Add(classifiedSequence);
                classifiedSequences.Add(string.Empty);
            }
            File.WriteAllLines(@"G:\Projects\HumanGenome\classifiedHelixSequences.txt", classifiedSequences);
        }

        private string GenerateAnnotatedSequence(IList<char> aminoAcidCodes, IList<bool> classificationResult)
        {
            var helixInProgress = false;
            var sequence = string.Empty;
            for (int codeIdx = 0; codeIdx < aminoAcidCodes.Count; codeIdx++)
            {
                var aminoAcidCode = aminoAcidCodes[codeIdx];
                var isHelix = classificationResult[codeIdx];
                if (!helixInProgress && isHelix)
                {
                    //sequence += "[";
                    helixInProgress = true;
                }
                else if (helixInProgress && !isHelix)
                {
                    //sequence += "]";
                    helixInProgress = false;
                }
                else
                {
                    //sequence += " ";
                }
                sequence += helixInProgress ? "#" : "_";
                //sequence += aminoAcidCode;
            }
            return sequence;
        }

        private readonly object fileLockObject = new object();
        private void WriteOptimizationResult(string filename, OptimizationResult optimizationResult)
        {
            lock (fileLockObject)
            {
                File.AppendAllText(filename,
                    optimizationResult.Cost + ";"
                    + optimizationResult.Iterations + ";"
                    + optimizationResult.Parameters
                        .Select(x => x.ToString("F4", CultureInfo.InvariantCulture))
                        .Aggregate((a, b) => a + ";" + b)
                    + Environment.NewLine
                );
            }
        }

        private void RandomizeStartValues(ParameterSetting[] parameterSettings, int startIdx)
        {
            for (int paramIdx = startIdx; paramIdx < parameterSettings.Length; paramIdx++)
            {
                parameterSettings[paramIdx].Start = 2*(StaticRandom.Rng.NextDouble() - 0.5);
            }
        }

        private List<string> AminoAcidCodePairs;
        private ParameterSetting[] GeneratePairwiseAminoAcidParameters()
        {
            var parameterSettings = new List<ParameterSetting>
            {
                new ParameterSetting("DirectNeighborWeight", -1e3, 1e3, 0.01, 0.1),
                new ParameterSetting("4thNeighborWeight", -1e3, 1e3, 0.01, 1.0)
            };
            var aminoAcids = (AminoAcidName[])Enum.GetValues(typeof(AminoAcidName));
            var singleLetterCodes = aminoAcids
                .Select(aa => aa.ToOneLetterCode())
                .OrderBy(x => x)
                .ToList();
            AminoAcidCodePairs = new List<string>();
            for (int idx1 = 0; idx1 < singleLetterCodes.Count; idx1++)
            {
                var code1 = singleLetterCodes[idx1];
                for (int idx2 = idx1; idx2 < singleLetterCodes.Count; idx2++)
                {
                    var code2 = singleLetterCodes[idx2];
                    var codePair = string.Empty + code1 + code2;
                    AminoAcidCodePairs.Add(codePair);
                    var parameterSetting = new ParameterSetting(
                        codePair,
                        -1,
                        1,
                        0.1,
                        1.0);
                    parameterSettings.Add(parameterSetting);
                }
            }
            return parameterSettings.ToArray();
        }

        private double HelixSequenceCostFunc(double[] parameters, List<HelixAnnotatedSequence> annotatedSequences)
        {
            var directNeighborInfluence = parameters[0];
            var fourthNeighborInfluence = parameters[1];
            var bondAffinityLookup = CreateBondAffinityLookup(parameters);
            var cost = 0.0;
            foreach (var annotatedSequence in annotatedSequences)
            {
                var classificationResult = ClassifySequence(annotatedSequence.AminoAcidCodes, bondAffinityLookup, directNeighborInfluence, fourthNeighborInfluence);
                var pairwiseCost = classificationResult.PairwiseOperation(annotatedSequence.IsHelixSignal, EvaluateClassification);
                cost += pairwiseCost.Sum();
            }
            cost += directNeighborInfluence*directNeighborInfluence + fourthNeighborInfluence*fourthNeighborInfluence;
            return cost;
        }

        //private double EvaluateClassification(bool classificationValue, bool expected)
        //{
        //    return classificationValue != expected ? 1 : 0;
        //}
        private static double EvaluateClassification(double classificationValue, bool expected)
        {
            var actual = classificationValue > 0;
            if (actual == expected)
            {
                return -0.5*Math.Max(1, Math.Abs(classificationValue));
            }
            return 1+Math.Abs(classificationValue);
        }

        private static IList<double> ClassifySequence(
            IList<char> aminoAcids,
            Dictionary<string, double> bondAffinityLookup,
            double directNeighborInfluence,
            double fourthNeighborInfluence)
        {
            var totalAffinity = new List<double>();
            for (int aminoAcidIdx = 0; aminoAcidIdx < aminoAcids.Count; aminoAcidIdx++)
            {
                var aminoAcid = aminoAcids[aminoAcidIdx];
                //var neighborM4 = aminoAcidIdx < 4 ? (char) 0 : aminoAcids[aminoAcidIdx - 4];
                var neighborM1 = aminoAcidIdx < 4 ? (char) 0 : aminoAcids[aminoAcidIdx - 1];
                var neighborP1 = aminoAcidIdx + 4 >= aminoAcids.Count ? (char) 0 : aminoAcids[aminoAcidIdx + 1];
                var neighborP4 = aminoAcidIdx + 4 >= aminoAcids.Count ? (char) 0 : aminoAcids[aminoAcidIdx + 4];

                var localAffinity = 0.0;
                //if(neighborM4 != 0)
                //{
                //    var pairCode = BuildAminoAcidPair(aminoAcid, neighborM4);
                //    if(bondAffinityLookup.ContainsKey(pairCode))
                //    {
                //        localAffinity += fourthNeighborInfluence * bondAffinityLookup[pairCode];
                //    }
                //}
                if (neighborM1 != 0)
                {
                    var pairCode = BuildAminoAcidPair(aminoAcid, neighborM1);
                    if (bondAffinityLookup.ContainsKey(pairCode))
                    {
                        localAffinity += directNeighborInfluence*bondAffinityLookup[pairCode];
                    }
                }
                if (neighborP1 != 0)
                {
                    var pairCode = BuildAminoAcidPair(aminoAcid, neighborP1);
                    if (bondAffinityLookup.ContainsKey(pairCode))
                    {
                        localAffinity += directNeighborInfluence * bondAffinityLookup[pairCode];
                    }
                }
                if (neighborP4 != 0)
                {
                    var pairCode = BuildAminoAcidPair(aminoAcid, neighborP4);
                    if (bondAffinityLookup.ContainsKey(pairCode))
                    {
                        localAffinity += fourthNeighborInfluence * bondAffinityLookup[pairCode];
                    }
                }
                totalAffinity.Add(localAffinity);
            }
            const int WindowSize = 21;
            var classification = totalAffinity.MovingAverage(WindowSize).ToList();
            return classification;
        }

        private static string BuildAminoAcidPair(char code1, char code2)
        {
            if (code1 < code2)
                return string.Empty + code1 + code2;
            return string.Empty + code2 + code1;
        }

        private Dictionary<string, double> CreateBondAffinityLookup(double[] parameters)
        {
            var lookup = new Dictionary<string, double>();
            for (int paramIdx = 2; paramIdx < parameters.Length; paramIdx++)
            {
                var parameter = parameters[paramIdx];
                lookup.Add(AminoAcidCodePairs[paramIdx-2], parameter);
            }
            return lookup;
        }

        private List<HelixAnnotatedSequence> ParseHelixSequences(string annotatedSequencesFile)
        {
            var annotatedSequences = new List<HelixAnnotatedSequence>();
            foreach (var line in File.ReadAllLines(annotatedSequencesFile))
            {
                if(line.StartsWith("#"))
                    continue;
                if(string.IsNullOrWhiteSpace(line))
                    continue;
                var aminoAcids = new List<char>();
                var annotation = new List<bool>();
                var isHelix = false;
                foreach (var c in line)
                {
                    if (c == '[')
                        isHelix = true;
                    else if (c == ']')
                        isHelix = false;
                    else
                    {
                        aminoAcids.Add(c);
                        annotation.Add(isHelix);
                    }
                }
                annotatedSequences.Add(new HelixAnnotatedSequence(annotation, aminoAcids));
            }
            return annotatedSequences;
        }
    }

    internal class HelixAnnotatedSequence
    {
        public HelixAnnotatedSequence(IList<bool> isHelixSignal, IList<char> aminoAcidCodes)
        {
            IsHelixSignal = isHelixSignal;
            AminoAcidCodes = aminoAcidCodes;
        }

        public IList<bool> IsHelixSignal { get; }
        public IList<char> AminoAcidCodes { get; }
    }
}
