using System.Collections.Generic;
using System.IO;

namespace GenomeTools.ChemistryLibrary.IO.Fasta
{
    public class FastaIndexReader
    {
        public List<FastaIndexEntry> ReadIndex(string indexFilePath)
        {
            return ReadIndex(File.OpenRead(indexFilePath));
        }

        public List<FastaIndexEntry> ReadIndex(Stream indexFileStream)
        {
            using var reader = new StreamReader(indexFileStream);
            var indexEntries = new List<FastaIndexEntry>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var indexEntry = ParseIndexEntry(line);
                if(indexEntry != null)
                    indexEntries.Add(indexEntry);
            }

            return indexEntries;
        }

        private FastaIndexEntry ParseIndexEntry(string line)
        {
            var splittedLine = line.Split('\t');
            if (splittedLine.Length < 5)
                return null;
            var sequenceName = splittedLine[0];
            var length = int.Parse(splittedLine[1]);
            var firstBaseOffset = long.Parse(splittedLine[2]);
            var basesPerLine = ushort.Parse(splittedLine[3]);
            var lineWidth = ushort.Parse(splittedLine[4]);
            return new FastaIndexEntry(sequenceName, firstBaseOffset, basesPerLine, lineWidth)
            {
                Length = length
            };
        }
    }
}
