using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Fasta
{
    public class FastaMd5HashFileReader
    {
        public List<FastaSequenceMd5Hash> Read(string filePath)
        {
            return File.ReadAllLines(filePath)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Split('\t'))
                .Where(x => x.Length == 2)
                .Select(x => new FastaSequenceMd5Hash(x[0], x[1]))
                .ToList();
        }
    }
}
