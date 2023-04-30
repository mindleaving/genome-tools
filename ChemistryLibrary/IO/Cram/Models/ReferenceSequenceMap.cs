using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.IO.Sam;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class ReferenceSequenceMap
    {
        private readonly Dictionary<string, int> nameToIndexMap;
        private readonly Dictionary<int, string> indexToNameMap;

        public ReferenceSequenceMap(Dictionary<string, int> nameToIndexMap, Dictionary<int,string> indexToNameMap)
        {
            this.nameToIndexMap = nameToIndexMap;
            this.indexToNameMap = indexToNameMap;
        }

        public static ReferenceSequenceMap FromSamHeaderEntries(IEnumerable<SamHeaderEntry> headerEntries)
        {
            var referenceSequenceHeaderEntries = headerEntries
                .Where(x => x.Type == SamHeaderEntry.HeaderEntryType.ReferenceSequence)
                .Cast<ReferenceSequenceSamHeaderEntry>();
            var indexToNameMap = new Dictionary<int, string>();
            var nameToIndexMap = new Dictionary<string, int>();
            var index = 0;
            foreach (var referenceSequenceHeaderEntry in referenceSequenceHeaderEntries)
            {
                indexToNameMap.Add(index, referenceSequenceHeaderEntry.ReferenceSequenceName);
                nameToIndexMap.Add(referenceSequenceHeaderEntry.ReferenceSequenceName, index);
                if (referenceSequenceHeaderEntry.AlternativeNames != null)
                {
                    foreach (var alternativeName in referenceSequenceHeaderEntry.AlternativeNames)
                    {
                        nameToIndexMap.Add(alternativeName, index);
                    }
                }
                index++;
            }
            return new ReferenceSequenceMap(nameToIndexMap, indexToNameMap);
        }

        public string GetSequenceNameFromIndex(int index)
        {
            return indexToNameMap[index];
        }

        public int GetIndexFromSequenceName(string sequenceName)
        {
            return nameToIndexMap[sequenceName];
        }
    }
}