using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Statistics.Testing;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Mathematics;

namespace Studies
{
    public class ProteinAligner
    {
        public LinearTransformation Align(Peptide peptide1, Peptide peptide2)
        {
            var logicalAlignment = Align(peptide1.AminoAcids, peptide2.AminoAcids);
        }

        public LinearTransformation AlignAtSequence(Peptide peptide1, Peptide peptide2, Range<int> aminoAcidRange)
        {
            var aminoAcidSequence1 = peptide1.AminoAcids.SubArray(aminoAcidRange.From, aminoAcidRange.To - aminoAcidRange.From + 1);
            var aminoAcidSequence2 = peptide2.AminoAcids
        }

        public SequenceAlignment<T> Align<T>(List<T> sequence1, List<T> sequence2)
        {
            var initialAlignment = SlideAlignment(sequence1, sequence2);
            GetOverlapOfOffsetSequences(sequence1, sequence2, initialAlignment, out var overlapSequence1, out var overlapSequence2);
            var overlapLength = overlapSequence1.Count;
            var alignmentPairs = Enumerable.Range(0, overlapLength)
                .Select(idx => new AlignedPair<T>(
                    overlapSequence1[idx], initialAlignment + idx, 
                    overlapSequence2[idx], initialAlignment + idx))
                .ToList();
            // TODO: Use dynamic programming or other sequence alignment algorithms for a better alignment. This initial alignment can maybe be used as a fix point.
            return new SequenceAlignment<T>(alignmentPairs);
        }

        private int SlideAlignment<T>(List<T> sequence1, List<T> sequence2)
        {
            var distinctItems = sequence1.Concat(sequence2).Distinct().Count();
            var overlapSums = new List<Tuple<int, double>>();
            for (int sequence2Offset = -(sequence2.Count-1); sequence2Offset <= sequence1.Count-1; sequence2Offset++)
            {
                GetOverlapOfOffsetSequences(sequence1, sequence2, sequence2Offset, out var overlapSequence1, out var overlapSequence2);
                var overlapLength = overlapSequence1.Count;

                var matchCount = 0;
                var lastWasMatch = false;
                var longestMatch = 0;
                var matchLength = 0;
                for (int overlapIdx = 0; overlapIdx < overlapLength; overlapIdx++)
                {
                    var sequence1Item = overlapSequence1[overlapIdx];
                    var sequence2Item = overlapSequence2[overlapIdx];
                    var isOverlap = sequence1Item.Equals(sequence2Item);
                    if (!isOverlap && lastWasMatch)
                    {
                        if (matchLength > longestMatch)
                            longestMatch = matchLength;
                        matchLength = 0;
                    }
                    if (isOverlap)
                    {
                        matchCount++;
                        matchLength++;
                    }
                    lastWasMatch = isOverlap;
                }

                var matchProbability = 1.0 / distinctItems;
                var matchCountSignificanceTest = new BinomialTest(
                    successes: matchCount, trials: overlapLength,
                    hypothesizedProbability: matchProbability,
                    alternate: OneSampleHypothesis.ValueIsGreaterThanHypothesis);
                var overlapLengthSignificanceTest = new BinomialTest(
                    successes: 1, trials: overlapLength - longestMatch+1,
                    hypothesizedProbability: Math.Pow(matchProbability, longestMatch),
                    alternate: OneSampleHypothesis.ValueIsGreaterThanHypothesis);
                var changeOfThisBeingRandom = Math.Min(matchCountSignificanceTest.PValue, overlapLengthSignificanceTest.PValue);
                overlapSums.Add(new Tuple<int, double>(sequence2Offset, changeOfThisBeingRandom));
            }
            var bestOverlapOffset = overlapSums.MinimumItem(x => x.Item2).Item1;
            return bestOverlapOffset;
        }

        /// <summary>
        /// Extract overlap of sequences when offsetting sequences. Resulting overlap sequences have the same length (by definition of 'overlap')
        /// </summary>
        /// <param name="sequence1">Sequence 1</param>
        /// <param name="sequence2">Sequence 2</param>
        /// <param name="offsetSequence2">Offset sequence 2 to the right by this much. Negative numbers mean offset to the left relative to sequence 1</param>
        /// <param name="overlapSequence1">Overlap of sequence 1</param>
        /// <param name="overlapSequence2">Overlap of sequenc 2</param>
        private void GetOverlapOfOffsetSequences<T>(
            List<T> sequence1,
            List<T> sequence2,
            int offsetSequence2,
            out List<T> overlapSequence1,
            out List<T> overlapSequence2)
        {
            var sequence1Start = Math.Max(0, offsetSequence2);
            var sequence1End = Math.Min(sequence1.Count - 1, sequence2.Count - 1 + offsetSequence2);
            var sequence2Start = Math.Max(0, -offsetSequence2);
            var sequence2End = Math.Min(sequence2.Count - 1, sequence1.Count - 1 - offsetSequence2);
            var overlapLength = sequence1End - sequence1Start + 1;
            overlapSequence1 = sequence1.Skip(sequence1Start).Take(overlapLength).ToList();
            overlapSequence2 = sequence2.Skip(sequence2Start).Take(overlapLength).ToList();
        }
    }

    public class SequenceAlignment<T>
    {
        public SequenceAlignment(List<AlignedPair<T>> alignedPairs)
        {
            AlignedPairs = alignedPairs;
        }

        public List<AlignedPair<T>> AlignedPairs { get; }
    }

    public class AlignedPair<T>
    {
        public AlignedPair(T item1, int item1Index, T item2, int item2Index)
        {
            Item1 = item1;
            Item2 = item2;
            Item1Index = item1Index;
            Item2Index = item2Index;
        }

        public int Item1Index { get; }
        public T Item1 { get; }

        public int Item2Index { get; }
        public T Item2 { get; }
    }
}
