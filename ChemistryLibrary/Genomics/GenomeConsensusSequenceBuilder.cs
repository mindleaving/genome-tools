using System;
using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.Measurements;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public class GenomeConsensusSequence
    {
        public GenomeConsensusSequence(
            IGenomeSequence primarySequence, 
            IGenomeSequence secondarySequence)
        {
            PrimarySequence = primarySequence;
            SecondarySequence = secondarySequence;
        }

        public IGenomeSequence PrimarySequence { get; }
        public IGenomeSequence SecondarySequence { get; }
    }
    public class GenomeConsensusSequenceBuilder
    {
        public GenomeConsensusSequence Build(GenomeSequenceAlignment alignment)
        {
            return Build(alignment.Reads, alignment.Chromosome, alignment.StartIndex, alignment.EndIndex);
        }

        public GenomeConsensusSequence Build(
            List<GenomeRead> reads, 
            string sequenceName,
            int referenceStartIndex,
            int referenceEndIndex)
        {
            if(!reads.Any())
            {
                var sequenceLength = referenceEndIndex - referenceStartIndex + 1;
                return new GenomeConsensusSequence(
                    new GenomeSequence(new string('N', sequenceLength), sequenceName, referenceStartIndex),
                    new GenomeSequence(new string('N', sequenceLength), sequenceName, referenceStartIndex));
            }
            var mappedReads = reads.Where(x => x.IsMapped);
            var positionOrderedReads = mappedReads.OrderBy(x => x.ReferenceStartIndex.Value);
            var readQueue = new Queue<GenomeRead>(positionOrderedReads);
            var readsInFrame = new List<GenomeRead>();
            var firstConsensusSequence = new List<char>();
            var secondConsensusSequence = new List<char>();
            for (int sequenceIndex = referenceStartIndex; sequenceIndex <= referenceEndIndex; sequenceIndex++)
            {
                // Add reads that have come into frame
                while (readQueue.Count > 0 && readQueue.Peek().ReferenceStartIndex.Value <= sequenceIndex)
                {
                    readsInFrame.Add(readQueue.Dequeue());
                }
                // Remove reads that have gone out of frame
                readsInFrame.RemoveAll(x => x.ReferenceEndIndex < sequenceIndex);

                var votes = readsInFrame
                    .Select(x => new NucleotideWithQualityScore(x.GetBaseAtReferencePosition(sequenceIndex), x.GetQualityScoreAtReferencePosition(sequenceIndex)))
                    .ToList();
                var nucleotideStatistics = new NucleotideStatistics(votes);
                firstConsensusSequence.Add(nucleotideStatistics.MostCommonNucleotide?.Nucleotide ?? 'N');
                var alternative = DetermineSecondaryNucleotide(nucleotideStatistics);
                secondConsensusSequence.Add(alternative);
            }

            return new GenomeConsensusSequence(
                new GenomeSequence(new string(firstConsensusSequence.ToArray()), sequenceName, referenceStartIndex),
                new GenomeSequence(new string(secondConsensusSequence.ToArray()), sequenceName, referenceStartIndex));
        }

        private char DetermineSecondaryNucleotide(NucleotideStatistics nucleotideStatistics)
        {
            if (nucleotideStatistics.MostCommonNucleotide == null)
                return 'N';
            if (nucleotideStatistics.SecondMostCommonNucleotide == null)
                return nucleotideStatistics.MostCommonNucleotide.Nucleotide;
            var mostCommonNucleotideRatio = nucleotideStatistics.MostCommonNucleotide.Count / (double)nucleotideStatistics.TotalCount;
            if (mostCommonNucleotideRatio >= 0.8)
                return nucleotideStatistics.MostCommonNucleotide.Nucleotide;
            return nucleotideStatistics.SecondMostCommonNucleotide.Nucleotide;
        }
    }

    public class NucleotideStatistics
    {
        public class NucleotideVoteResult
        {
            public NucleotideVoteResult(char nucleotide, double logQualityScoreSum, int count)
            {
                Nucleotide = nucleotide;
                LogQualityScoreSum = logQualityScoreSum;
                Count = count;
            }

            public char Nucleotide { get; }
            public int Count { get; }

            /// <summary>
            /// Sum of log of error probabilities,
            /// i.e. the sum of log(Pi) = -Q/10 where Q is the phred quality score
            /// </summary>
            public double LogQualityScoreSum { get; }
        }
        public NucleotideStatistics(IEnumerable<NucleotideWithQualityScore> votes)
        {
            var statistics = votes
                .GroupBy(x => x.Nucleotide)
                .Select(x => new NucleotideVoteResult(
                    x.Key, 
                    x.Sum(vote => -vote.QualityScore/10d),
                    x.Count())
                )
                .OrderBy(x => x.LogQualityScoreSum)
                .ToList();
            MostCommonNucleotide = statistics.FirstOrDefault();
            SecondMostCommonNucleotide = statistics.Count > 1 
                ? statistics[1]
                : null;
            TotalCount = statistics.Sum(x => x.Count);
        }

        public NucleotideVoteResult MostCommonNucleotide { get; }
        public NucleotideVoteResult SecondMostCommonNucleotide { get; }
        public int TotalCount { get; }
    }
}