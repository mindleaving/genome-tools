using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Pdb
{
    /// <summary>
    /// Matches two sequences which are identical except for 
    /// their start and ends, which might be missing
    /// </summary>
    public class OffsetSequenceMatcher<T>
    {
        public OffsetSequenceMatcher(IList<T> sequence1, IList<T> sequence2)
        {
            if(!sequence1.Any())
                throw new ArgumentException("Sequence 1 is empty");
            if (!sequence2.Any())
                throw new ArgumentException("Sequence 2 is empty");
            Sequence1 = sequence1;
            Sequence2 = sequence2;

            var shortestSequenceLength = Math.Min(sequence1.Count, sequence2.Count);
            var sequence1FirstElement = sequence1.First();
            var firstElementPositionsInSequence2 = sequence2
                .Select((item, idx) => new {Index = idx, Item = item})
                .Where(x => x.Item.Equals(sequence1FirstElement))
                .Select(x => x.Index);

            // Slide sequence 1 along sequence 2
            var bestOveralpPercentage = 0.0;
            var bestMatchPosition = new Tuple<int, int>(0, 0);
            foreach (var sequence2Start in firstElementPositionsInSequence2)
            {
                var overlapLength = Math.Min(sequence1.Count, sequence2.Count - sequence2Start);
                var overlapPercentage = 100*overlapLength/shortestSequenceLength;
                var isMatch = IsMatch(
                    GetSubarray(sequence1, 0, overlapLength),
                    GetSubarray(sequence2, sequence2Start, overlapLength));
                if(!isMatch)
                    continue;
                if (overlapPercentage > bestOveralpPercentage)
                {
                    bestMatchPosition = new Tuple<int, int>(0, sequence2Start);
                    bestOveralpPercentage = overlapPercentage;
                }
            }

            // Slide sequence 2 along sequence 1
            var sequence2FirstElement = sequence2.First();
            var firstElementPositionsInSequence1 = sequence1
                .Select((item, idx) => new {Index = idx, Item = item})
                .Where(x => x.Item.Equals(sequence2FirstElement))
                .Select(x => x.Index);
            foreach (var sequence1Start in firstElementPositionsInSequence1)
            {
                var overlapLength = Math.Min(sequence1.Count - sequence1Start, sequence2.Count);
                var overlapPercentage = 100*overlapLength / shortestSequenceLength;
                var isMatch = IsMatch(
                    GetSubarray(sequence1, sequence1Start, overlapLength),
                    GetSubarray(sequence2, 0, overlapLength));
                if (!isMatch)
                    continue;
                if (overlapPercentage > bestOveralpPercentage)
                {
                    bestMatchPosition = new Tuple<int, int>(sequence1Start, 0);
                    bestOveralpPercentage = overlapPercentage;
                }
            }

            OverlapPercentage = bestOveralpPercentage;
            SequenceOffset = bestMatchPosition;
            CombinedSequence = BuildCombinedSequence(sequence1, sequence2, SequenceOffset);
        }

        private IList<T> BuildCombinedSequence(IList<T> sequence1, IList<T> sequence2, Tuple<int, int> sequenceOffset)
        {
            var combinedSequence = new List<T>();
            if (sequenceOffset.Item1 == 0)
            {
                combinedSequence.AddRange(sequence2.Take(sequenceOffset.Item2));
                combinedSequence.AddRange(sequence1);
                combinedSequence.AddRange(sequence2.Skip(sequenceOffset.Item2 + sequence1.Count));
            }
            else
            {
                combinedSequence.AddRange(sequence1.Take(sequenceOffset.Item1));
                combinedSequence.AddRange(sequence2);
                combinedSequence.AddRange(sequence1.Skip(sequenceOffset.Item1 + sequence2.Count));
            }
            return combinedSequence;
        }

        private bool IsMatch(IList<T> sequence1, IList<T> sequence2)
        {
            if(sequence1.Count != sequence2.Count)
                throw new ArgumentException("Sequences must be of equal length");
            for (int idx = 0; idx < sequence1.Count; idx++)
            {
                if (!sequence1[idx].Equals(sequence2[idx]))
                    return false;
            }
            return true;
        }

        private IList<T> GetSubarray(IList<T> sequence, int sequenceStart, int sequenceLength)
        {
            return sequence.Skip(sequenceStart).Take(sequenceLength).ToList();
        }

        public IList<T> Sequence1 { get; }
        public IList<T> Sequence2 { get; }
        public double OverlapPercentage { get; }
        public Tuple<int, int> SequenceOffset { get; }
        public IList<T> CombinedSequence { get; }
    }
}
