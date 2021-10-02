using System.IO;
using GenomeTools.ChemistryLibrary.IO;
using NUnit.Framework;

namespace GenomeTools.Tools
{
    public class ChromosomeDataExport
    {
        private const string OutputDirectory = @"D:\HumanGenome\chromosomeExtracts";

        [Test]
        [TestCase("17", 8224821, 8248044)]
        [TestCase("17", 8227982, 8229405)]
        [TestCase("17", 8228549, 8230397)]
        [TestCase("17", 9086924-20000, 9086924+20000)]
        [TestCase("17", 6299212-20000, 6299212+20000)]
        public void Export(string chromosomeName, int startIndex, int endIndex)
        {
            var chromosomeDataLoader = new ChromosomeDataLoader(DataLocations.Chromosomes);
            var nucleotides = chromosomeDataLoader.Load(chromosomeName, startIndex, endIndex);
            File.AppendAllText(
                Path.Combine(OutputDirectory, $"{chromosomeName}_{startIndex}_{endIndex}.txt"),
                nucleotides);
        }
    }
}
