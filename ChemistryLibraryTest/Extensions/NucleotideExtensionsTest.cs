using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Extensions
{
    public class NucleotideExtensionsTest
    {
        [Test]
        [TestCase(Nucleotide.A, Nucleotide.T)]
        [TestCase(Nucleotide.C, Nucleotide.G)]
        [TestCase(Nucleotide.G, Nucleotide.C)]
        [TestCase(Nucleotide.T, Nucleotide.A)]
        [TestCase(Nucleotide.W, Nucleotide.W)]
        [TestCase(Nucleotide.S, Nucleotide.S)]
        public void ToComplementReturnsComplement(Nucleotide input, Nucleotide expected)
        {
            Assert.That(input.ToComplement(), Is.EqualTo(expected));
            Assert.That(NucleotideExtensions.IsComplementaryMatch(input, expected));
        }
    }
}
