using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfIndexReader
    {
        private readonly string filePath;
        private readonly Dictionary<string, string> sequenceNameTranslation;

        public VcfIndexReader(string filePath, Dictionary<string, string> sequenceNameTranslation = null)
        {
            this.filePath = filePath;
            this.sequenceNameTranslation = sequenceNameTranslation;
        }
        public List<VcfIndexEntry> Read()
        {
            return File.ReadLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => ParseEntry(line, sequenceNameTranslation))
                .ToList();
        }

        private static VcfIndexEntry ParseEntry(string line, IReadOnlyDictionary<string, string> sequenceNameTranslation)
        {
            var splittedLine = line.Split('\t');
            var chromosome = sequenceNameTranslation != null && sequenceNameTranslation.ContainsKey(splittedLine[0])
                ? sequenceNameTranslation[splittedLine[0]]
                : splittedLine[0];
            var position = int.Parse(splittedLine[1]);
            var fileOffset = long.Parse(splittedLine[2]);
            return new VcfIndexEntry(chromosome, position, fileOffset);
        }
    }
}
