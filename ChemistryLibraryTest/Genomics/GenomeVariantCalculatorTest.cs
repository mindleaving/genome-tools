using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO.Fastq;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Genomics
{
    public class GenomeVariantCalculatorTest
    {
        [Test]
        public void VariantIsDetected()
        {
            var reference = "AAAACCCCTTTTGGGG";
            var reads = new FastqReader().Read(new MemoryStream(Encoding.ASCII.GetBytes(
                "@r1\n" +
                "AAAACCCCTTTGGGGG\n" +
                "+\n" + 
                "$$$$$$$$$$$A$$$$\n" +
                "@r2\n" +
                "AAAACCCCTTTCGGGG\n" +
                "+\n" + 
                "$$$$$$$$$$$$$$$$\n" +
                "@r3\n" +
                "AAAACCCCTTTCGGGG\n" +
                "+\n" + 
                "$$$$$$$$$$$$$$$$\n"
            )));
            var sut = new GenomeVariantCalculator();
            for (int i = 0; i < reference.Length; i++)
            {
                var referenceNucleotide = reference[i];
                var readNucleotides = reads.Select(x => new NucleotideWithQualityScore(x.GetBaseAtPosition(i), x.GetQualityScoreAtPosition(i))).ToList();
                var actual = sut.CalculateVariantProbability(referenceNucleotide, readNucleotides);
                Console.WriteLine(
                    $"{referenceNucleotide} {string.Join("", readNucleotides.Select(x => x.Nucleotide))}: "
                    + $"Homozygous: {actual.HomozygousProbability * 100:F2}%, "
                    + $"Heterozygous: {actual.HeterozygousProability * 100:F2}%");
            }
        }
    }
}
