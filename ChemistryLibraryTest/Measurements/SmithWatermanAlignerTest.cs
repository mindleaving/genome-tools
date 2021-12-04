using System;
using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Measurements
{
    public class SmithWatermanAlignerTest
    {
        [Test]
        public void NonMatchingSingleLetterReturnEmptyResult()
        {
            var sequence1 = "T";
            var sequence2 = "G";
            var sut = new SmithWatermanAligner<char>();

            var actual = sut.Align(sequence1.ToCharArray(), sequence2.ToCharArray(), (a, b) => a == b);

            Assert.That(actual.Matches.Count, Is.EqualTo(0));
        }

        [Test]
        public void SingleLetterIsAlignedToCorrectPosition()
        {
            var sequence1 = "A";
            var sequence2 = "CTAG";
            var sut = new SmithWatermanAligner<char>();

            var actual = sut.Align(sequence1.ToCharArray(), sequence2.ToCharArray(), (a, b) => a == b);

            Assert.That(actual.Matches.Count, Is.EqualTo(1));
            Assert.That(actual.Matches[0].Sequence1StartIndex, Is.EqualTo(0));
            Assert.That(actual.Matches[0].Sequence2StartIndex, Is.EqualTo(2));
            Assert.That(actual.Matches[0].Sequence1, Is.EqualTo("A"));

            // Switch sequences
            actual = sut.Align(sequence2.ToCharArray(), sequence1.ToCharArray(), (a, b) => a == b);

            Assert.That(actual.Matches.Count, Is.EqualTo(1));
            Assert.That(actual.Matches[0].Sequence1StartIndex, Is.EqualTo(2));
            Assert.That(actual.Matches[0].Sequence2StartIndex, Is.EqualTo(0));
            Assert.That(actual.Matches[0].Sequence2, Is.EqualTo("A"));
        }

        /// <summary>
        /// See https://en.wikipedia.org/wiki/Smith%E2%80%93Waterman_algorithm
        /// </summary>
        [Test]
        public void WikipediaExampleIsAlignedAsExpected()
        {
            var sequence1 = "TGTTACGG";
            var sequence2 = "GGTTGACTA";
            var sut = new SmithWatermanAligner<char>();

            var actual = sut.Align(sequence1.ToCharArray(), sequence2.ToCharArray(), (a, b) => a == b);

            Assert.That(actual.Matches.Count, Is.EqualTo(2));
            var orderedMatches = actual.Matches.OrderBy(x => x.Sequence1StartIndex).ToList();
            var firstMatch = orderedMatches[0];
            Assert.That(firstMatch.Sequence1StartIndex, Is.EqualTo(1));
            Assert.That(firstMatch.Sequence2StartIndex, Is.EqualTo(1));
            Assert.That(firstMatch.Sequence1, Is.EqualTo("GTT"));
            var secondMatch = orderedMatches[1];
            Assert.That(secondMatch.Sequence1StartIndex, Is.EqualTo(4));
            Assert.That(secondMatch.Sequence2StartIndex, Is.EqualTo(5));
            Assert.That(secondMatch.Sequence1, Is.EqualTo("AC"));
        }

        [Test]
        public void ModifiedWikipediaExampleIsAlignedAsExpected()
        {
            var sequence1 = "GTTACGG"; // Removed first nucleotide
            var sequence2 = "GGTTGACTA";
            var sut = new SmithWatermanAligner<char>();

            var actual = sut.Align(sequence1.ToCharArray(), sequence2.ToCharArray(), (a, b) => a == b);

            Assert.That(actual.Matches.Count, Is.EqualTo(2));
            var orderedMatches = actual.Matches.OrderBy(x => x.Sequence1StartIndex).ToList();
            var firstMatch = orderedMatches[0];
            Assert.That(firstMatch.Sequence1StartIndex, Is.EqualTo(0));
            Assert.That(firstMatch.Sequence2StartIndex, Is.EqualTo(1));
            Assert.That(firstMatch.Sequence1, Is.EqualTo("GTT"));
            var secondMatch = orderedMatches[1];
            Assert.That(secondMatch.Sequence1StartIndex, Is.EqualTo(3));
            Assert.That(secondMatch.Sequence2StartIndex, Is.EqualTo(5));
            Assert.That(secondMatch.Sequence1, Is.EqualTo("AC"));
        }

        [Test]
        public void BadlyMatchingSequencesDontThrowException()
        {
            var sequence1 = "TGGCACGTGCG";
            var sequence2 = "GAGTCTACC";
            var sut = new SmithWatermanAligner<char>();

            var actual = sut.Align(sequence1.ToCharArray(), sequence2.ToCharArray(), (a, b) => a == b);

            Assert.That(actual.Matches.Count, Is.GreaterThan(0));
        }

        [Test]
        public void RealWorldSequence1()
        {
            var sequence1 = ToNucleotides("GTGAGTGGCAGGGCGCTTGGGACTCTGGGGAGGCCTTGGGTGGCGATGGGAGCGCTGGGGTAAGTGTCCTTTTCTCCTCTCCAG");
            var sequence2 = sequence1.AsEnumerable().Reverse().ToList();
            var sut = new SmithWatermanAligner<Nucleotide>();

            var actual = sut.Align(sequence1, sequence2, NucleotideExtensions.IsComplementaryMatch);

            Assert.That(actual.Matches.Count, Is.EqualTo(16));
            foreach (var match in actual.Matches)
            {
                Assert.That(match.Sequence1, Is.EqualTo(match.Sequence2.Select(NucleotideExtensions.ToComplement)));
            }
        }

        private static List<Nucleotide> ToNucleotides(string nucleotides)
        {
            return nucleotides.Select(nucleotide => Enum.Parse<Nucleotide>(nucleotide + "")).ToList();
        }
    }
}
