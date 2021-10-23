using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Index
{
    public class CramIndex
    {
        private readonly Dictionary<int, List<CramIndexEntry>> sequenceIndexEntries;

        public CramIndex(List<CramIndexEntry> indexEntries)
        {
            sequenceIndexEntries = indexEntries
                .GroupBy(x => x.ReferenceSequenceId)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public List<CramIndexEntry> GetEntriesForReferenceSequence(int sequenceId)
        {
            if (!sequenceIndexEntries.ContainsKey(sequenceId))
                return new List<CramIndexEntry>();
            return sequenceIndexEntries[sequenceId];
        }
    }
}