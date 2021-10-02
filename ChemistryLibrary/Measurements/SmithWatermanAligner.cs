using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public class AlignmentResult<T>
    {
        public AlignmentResult(List<AlignmentMatch<T>> matches)
        {
            Matches = matches;
        }

        public List<AlignmentMatch<T>> Matches { get; }
    }

    public class AlignmentMatch<T>
    {
        public AlignmentMatch(
            int sequence1StartIndex,
            IList<T> sequence1,
            int sequence2StartIndex,
            IList<T> sequence2)
        {
            if (sequence1.Count != sequence2.Count)
                throw new ArgumentException("Aligned sequences do not have the same length");
            Sequence1StartIndex = sequence1StartIndex;
            Sequence1 = sequence1;
            Sequence2StartIndex = sequence2StartIndex;
            Sequence2 = sequence2;
        }

        public int Sequence1StartIndex { get; }
        public IList<T> Sequence1 { get; }
        public int Sequence2StartIndex { get; }
        public IList<T> Sequence2 { get; }
        public int Length => Sequence1.Count;
    }

    public class SmithWatermanAlignerOptions
    {
        public ushort GapPenalty { get; set; } = 2;
        public ushort MatchReward { get; set; } = 3;
    }
    public class SmithWatermanAligner<T>
    {
        public AlignmentResult<T> Align(
            IList<T> sequence1,
            IList<T> sequence2,
            Func<T,T,bool> itemComparer,
            SmithWatermanAlignerOptions options = null)
        {
            if (options == null)
                options = new SmithWatermanAlignerOptions();

            var (substitutionMatrix, maxScoreRowIndex, maxScoreColumnIndex) = CalculateSubstitutionMatrix(
                sequence1, 
                sequence2,
                itemComparer,
                options);

            return TraceBack(substitutionMatrix, maxScoreRowIndex, maxScoreColumnIndex, sequence1, sequence2, itemComparer);
        }

        private (ushort[,] substitutionMatrix, int maxScoreRowIndex, int maxScoreColumnIndex) CalculateSubstitutionMatrix(
            IList<T> sequence1,
            IList<T> sequence2,
            Func<T, T, bool> itemComparer,
            SmithWatermanAlignerOptions options)
        {
            var substitutionMatrix = new ushort[sequence2.Count + 1, sequence1.Count + 1];
            ushort maxScore = 0;
            var maxScoreRowIndex = 0;
            var maxScoreColumnIndex = 0;
            for (var rowIndex = 1; rowIndex <= sequence2.Count; rowIndex++)
            {
                for (var columnIndex = 1; columnIndex <= sequence1.Count; columnIndex++)
                {
                    var sequence1Index = columnIndex - 1;
                    var sequence2Index = rowIndex - 1;
                    var sequence1Item = sequence1[sequence1Index];
                    var sequence2Item = sequence2[sequence2Index];
                    var isMatch = itemComparer(sequence1Item, sequence2Item);

                    var horizontalMove = CalculateHorizontalMoveScore(
                        substitutionMatrix,
                        rowIndex,
                        columnIndex,
                        options.GapPenalty);
                    var verticalMove = CalculateVerticalMoveScore(
                        substitutionMatrix,
                        rowIndex,
                        columnIndex,
                        options.GapPenalty);
                    var diagonalMove = CalculateDiagonalMoveScore(
                        substitutionMatrix,
                        rowIndex,
                        columnIndex,
                        isMatch,
                        options.MatchReward);

                    var score = Max(
                        horizontalMove,
                        verticalMove,
                        diagonalMove,
                        0);
                    substitutionMatrix[rowIndex, columnIndex] = score;
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxScoreRowIndex = rowIndex;
                        maxScoreColumnIndex = columnIndex;
                    }
                }
            }

            return (substitutionMatrix, maxScoreRowIndex, maxScoreColumnIndex);
        }

        private AlignmentResult<T> TraceBack(
            ushort[,] substitutionMatrix,
            int maxScoreRowIndex,
            int maxScoreColumnIndex,
            IList<T> sequence1,
            IList<T> sequence2,
            Func<T, T, bool> itemComparer)
        {
            var score = substitutionMatrix[maxScoreRowIndex, maxScoreColumnIndex];
            var rowIndex = maxScoreRowIndex;
            var columnIndex = maxScoreColumnIndex;
            var isMatchInProgress = true;
            var currentMatchSequence1EndIndex = columnIndex - 1; // -1 because substitution matrix has additional row and column
            var currentMatchSequence2EndIndex = rowIndex - 1;

            var matches = new List<AlignmentMatch<T>>();
            while (score > 0)
            {
                var leftValue = substitutionMatrix[rowIndex, columnIndex - 1];
                var aboveValue = substitutionMatrix[rowIndex - 1, columnIndex];
                var diagonalValue = substitutionMatrix[rowIndex - 1, columnIndex - 1];
                if (diagonalValue == 0)
                {
                    rowIndex--;
                    columnIndex--;
                    break;
                }
                if(leftValue == 0 || aboveValue == 0)
                    break;
                if (diagonalValue >= aboveValue && diagonalValue >= leftValue)
                {
                    var isMatch = itemComparer(sequence1[columnIndex - 1], sequence2[rowIndex - 1]);
                    if (isMatch && !isMatchInProgress)
                    {
                        isMatchInProgress = true;
                        currentMatchSequence1EndIndex = columnIndex - 1;
                        currentMatchSequence2EndIndex = rowIndex - 1;
                    }
                    else if (!isMatch && isMatchInProgress)
                    {
                        var match = BuildMatch(
                            sequence1, 
                            sequence2,
                            rowIndex,
                            columnIndex,
                            currentMatchSequence1EndIndex,
                            currentMatchSequence2EndIndex,
                            itemComparer);
                        matches.Add(match);
                        isMatchInProgress = false;
                    }
                    score = diagonalValue;
                    rowIndex--;
                    columnIndex--;
                }
                else
                {
                    if (isMatchInProgress)
                    {
                        var match = BuildMatch(
                            sequence1, 
                            sequence2,
                            rowIndex,
                            columnIndex,
                            currentMatchSequence1EndIndex,
                            currentMatchSequence2EndIndex,
                            itemComparer);
                        matches.Add(match);
                        isMatchInProgress = false;
                    }

                    if (aboveValue > leftValue)
                    {
                        score = aboveValue;
                        rowIndex--;
                    }
                    else
                    {
                        score = leftValue;
                        columnIndex--;
                    }
                }
            }
            if (isMatchInProgress)
            {
                var match = BuildMatch(
                    sequence1, 
                    sequence2,
                    rowIndex,
                    columnIndex,
                    currentMatchSequence1EndIndex,
                    currentMatchSequence2EndIndex,
                    itemComparer);
                matches.Add(match);
            }

            return new AlignmentResult<T>(matches.OrderBy(x => x.Sequence1StartIndex).ToList());
        }

        private static AlignmentMatch<T> BuildMatch(
            IList<T> sequence1,
            IList<T> sequence2,
            int rowIndex,
            int columnIndex,
            int currentMatchSequence1EndIndex,
            int currentMatchSequence2EndIndex,
            Func<T, T, bool> itemComparer)
        {
            var currentMatchSequence1StartIndex = columnIndex; // Move one diagonal back (+1) and take into account extrac substitution matrix row/column (-1)
            var currentMatchSequence2StartIndex = rowIndex;
            var matchedSequence1 = sequence1.SubArray(new Range<int>(currentMatchSequence1StartIndex, currentMatchSequence1EndIndex));
            var matchedSequence2 = sequence2.SubArray(new Range<int>(currentMatchSequence2StartIndex, currentMatchSequence2EndIndex));
#if DEBUG
            if (!matchedSequence1.PairwiseOperation(matchedSequence2, itemComparer).All(x => x))
                throw new Exception("Matched sequences are not matches");
#endif
            var match = new AlignmentMatch<T>(
                currentMatchSequence1StartIndex,
                matchedSequence1,
                currentMatchSequence2StartIndex,
                matchedSequence2);
            return match;
        }

        private static ushort Max(params ushort[] values)
        {
            return values.Max();
        }

        private ushort CalculateDiagonalMoveScore(
            ushort[,] substitutionMatrix,
            int rowIndex,
            int columnIndex,
            bool isMatch,
            ushort matchReward)
        {
            var diagonalValue = substitutionMatrix[rowIndex - 1, columnIndex - 1];
            if (isMatch)
                return (ushort) (diagonalValue + matchReward);
            if (diagonalValue <= matchReward)
                return 0;
            return (ushort) (diagonalValue - matchReward);
        }

        private ushort CalculateVerticalMoveScore(
            ushort[,] substitutionMatrix,
            int rowIndex,
            int columnIndex,
            ushort gapPenalty)
        {
            var aboveValue = substitutionMatrix[rowIndex - 1, columnIndex];
            if (aboveValue <= gapPenalty)
                return 0;
            return (ushort) (aboveValue - gapPenalty);
        }

        private ushort CalculateHorizontalMoveScore(
            ushort[,] substitutionMatrix,
            int rowIndex,
            int columnIndex,
            ushort gapPenalty)
        {
            var leftValue = substitutionMatrix[rowIndex, columnIndex - 1];
            if (leftValue <= gapPenalty)
                return 0;
            return (ushort)(leftValue - gapPenalty);
        }
    }
}