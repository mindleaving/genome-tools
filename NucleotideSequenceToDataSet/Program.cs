using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NucleotideSequenceToDataSet
{
    public static class Program
    {
        private const string ChromosomeDataDirectory = @"G:\Projects\HumanGenome\chromosomes";
        private const int ExonBoundaryLength = 25;

        public static void Main()
        {
            var nucleotideSequence = ReadNucleotidesFromFile("7", 117479963, 117668665);
            var dataSet = new List<string>();
            for (var nucleotideIdx = 0; nucleotideIdx < nucleotideSequence.Length-2*ExonBoundaryLength; nucleotideIdx++)
            {
                var subSequence = nucleotideSequence
                    .Substring(nucleotideIdx, 2*ExonBoundaryLength)
                    .Select(c => c + "")
                    .Aggregate((a,b) => a + "," + b);
                dataSet.Add(subSequence);
            }
            File.WriteAllLines($@"dataset_length{ExonBoundaryLength}.csv", dataSet);
        }

        private static string ReadNucleotidesFromFile(string chromosome, int startBase, int endBase)
        {
            var chromosomeFile = Directory.GetFiles(ChromosomeDataDirectory, $"*chromosome_{chromosome}.*").Single();
            var chromosomeData = File.ReadAllText(chromosomeFile);
            return chromosomeData.Substring(startBase - 1, endBase - startBase + 1);
        }
    }
}
