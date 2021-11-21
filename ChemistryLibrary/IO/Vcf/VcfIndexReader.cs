using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfIndexReader
    {
        public List<VcfIndexEntry> Read(string filePath)
        {
            return File.ReadLines(filePath).Where(line => !string.IsNullOrWhiteSpace(line)).Select(ParseEntry).ToList();
        }

        private VcfIndexEntry ParseEntry(string line)
        {
            var splittedLine = line.Split('\t');
            return new VcfIndexEntry(splittedLine[0], int.Parse(splittedLine[1]), long.Parse(splittedLine[2]));
        }
    }
}
