using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class ChromosomeDataLoader
    {
        private readonly string chromosomeDataDirectory;

        public ChromosomeDataLoader(string chromosomeDataDirectory)
        {
            this.chromosomeDataDirectory = chromosomeDataDirectory;
        }

        public string Load(GeneLocationInfo gene)
        {
            return Load(gene.Chromosome, gene.StartBase, gene.EndBase);
        }

        /// <summary>
        /// Load nucleotide sequence from chromosome at specified location
        /// </summary>
        /// <param name="chromosomeName">Name of chromosome, e.g. "1", "MT", "X"</param>
        /// <param name="startBase">IMPORTANT! One-based index of first base</param>
        /// <param name="endBase">One-based index of last base to include</param>
        /// <returns></returns>
        public string Load(
            string chromosomeName,
            int startBase,
            int endBase)
        {
            var chromosomeFilePath = GetChromosomeFilePath(chromosomeName);
            var buffer = new char[endBase - startBase + 1];
            using var streamReader = new StreamReader(chromosomeFilePath);
            streamReader.BaseStream.Seek(startBase - 1, SeekOrigin.Begin);
            streamReader.Read(buffer, 0, buffer.Length);
            return new string(buffer);
        }

        public string Load(string chromosomeName)
        {
            var chromosomeFilePath = GetChromosomeFilePath(chromosomeName);
            return File.ReadAllText(chromosomeFilePath);
        }

        private string GetChromosomeFilePath(string chromosomeName)
        {
            return Directory.GetFiles(chromosomeDataDirectory, $"*chromosome_{chromosomeName}.*").Single();
        }
    }
}
