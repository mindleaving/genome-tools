using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.Measurements;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenomeVariantCalculator
    {
        public GenomeVariantProbability CalculateVariantProbability(char referenceBase, List<NucleotideWithQualityScore> votes)
        {
            var scoreMap = "ACGTN".ToDictionary(c => c, _ => 0.0);

            foreach (var vote in votes)
            {
                var nucleotide = vote.Nucleotide;
                var logErrorProbability = LogErrorProbabilityFromQualitScore(vote.QualityScore);
                scoreMap[nucleotide] += logErrorProbability;
            }

            var mostLikelyNucleotide = scoreMap.MinimumItem(x => x.Value);
            var otherNucleotides = scoreMap
                .Where(kvp => kvp.Key != mostLikelyNucleotide.Key)
                .ToList();
            var averageValueOtherNucleotides = otherNucleotides.Average(kvp => kvp.Value);
            if (mostLikelyNucleotide.Value > 2 * averageValueOtherNucleotides)
                return new GenomeVariantProbability(0, 0); // TODO: No nucleotide was particularly likely. 

            const double EquallyLikelyThreshold = 0.8;
            var otherProbableNucleotides = otherNucleotides
                .Where(kvp => kvp.Value < EquallyLikelyThreshold * mostLikelyNucleotide.Value)
                .ToList();
            var isHeterozgous = otherProbableNucleotides.Any();
            if (isHeterozgous)
            {
                return new GenomeVariantProbability(0, 1); // TODO
            }
            else
            {
                if (mostLikelyNucleotide.Key == referenceBase)
                {
                    return new GenomeVariantProbability(0, 0); // TODO
                }
                return new GenomeVariantProbability(1, 0); // TODO
            }
        }

        public static double LogErrorProbabilityFromQualitScore(char phredQualityScore)
        {
            var q = phredQualityScore - 33;
            return - q / 10.0;
        }

        public static double ErrorProbabilityFromQualityScore(char phredQualityScore)
        {
            var q = phredQualityScore - 33;
            var p = Math.Pow(10, -q / 10.0);
            return p;
        }
    }
}