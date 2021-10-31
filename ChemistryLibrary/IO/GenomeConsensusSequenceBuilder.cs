using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeConsensusSequenceBuilder
    {
        public IGenomeSequence Build(GenomeSequenceAlignment alignment)
        {
            var mappedReads = alignment.Reads.Where(x => x.IsMapped);
            var positionOrderedReads = mappedReads.OrderBy(x => x.ReadPosition.Value);
            var readQueue = new Queue<GenomeRead>(positionOrderedReads);
            var sequenceStartIndex = readQueue.First().ReadPosition.Value;
            var sequenceEndIndex = readQueue.Select(x => x.ReadPosition.Value + x.Length).Max();
            var readsInFrame = new List<GenomeRead>();
            var consensusSequence = new List<char>();
            for (int sequenceIndex = sequenceStartIndex; sequenceIndex < sequenceEndIndex; sequenceIndex++)
            {
                // Add reads that have come into frame
                while (readQueue.Count > 0 && readQueue.Peek().ReadPosition.Value <= sequenceIndex)
                {
                    readsInFrame.Add(readQueue.Dequeue());
                }
                // Remove reads that have gone out of frame
                readsInFrame.RemoveAll(x => x.ReadPosition.Value + x.Length - 1 < sequenceIndex);

                var votes = readsInFrame.Select(x => x.GetBaseAtPosition(sequenceIndex)).ToList();
                var nucleotideStatistics = new NucleotideStatistics(votes);
                consensusSequence.Add(nucleotideStatistics.MostCommonNucleotide);
            }

            return new GenomeSequence(new string(consensusSequence.ToArray()), alignment.ReferenceSequence, sequenceStartIndex);
        }
    }

    public class NucleotideStatistics
    {
        private readonly Dictionary<char,int> statistics;

        public NucleotideStatistics(IEnumerable<char> nucleotides)
        {
            statistics = nucleotides.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
            MostCommonNucleotide = statistics.MaximumItem(x => x.Value).Key;
        }

        public char MostCommonNucleotide { get; }

        public int GetCount(char nucleotide)
        {
            if (!statistics.ContainsKey(nucleotide))
                return 0;
            return statistics[nucleotide];
        }
    }
}