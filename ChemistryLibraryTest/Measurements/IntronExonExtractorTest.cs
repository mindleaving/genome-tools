using System;
using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Measurements
{
    public class IntronExonExtractorTest
    {
        private const int MinimumExonLength = 2;

        [Test]
        [Ignore("Tool")]
        public void GenerateNucleotideSeqeunceForAminoAcids()
        {
            var aminoAcids = "KLYNGTI".Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName());
            var nucleotides = string.Join(string.Empty, aminoAcids.Select(CodonMap.GenerateCodonForAminoAcid));
            Console.WriteLine(nucleotides);
        }

        [Test]
        public void EmptyAminoAcidSequenceThrowsArgumentException()
        {
            Assert.That(() => new IntronExonExtractor("AAA", "", MinimumExonLength), Throws.ArgumentException);
        }

        [Test]
        public void NucleotideSequenceShorterThanThreeTimesAminoAcidSeqeuncePlusStopCodonThrowsArgumentException()
        {
            Assert.That(() => new IntronExonExtractor("AAATTTGGGCC", "MTY", MinimumExonLength), Throws.ArgumentException);
        }

        [Test]
        public void StopCodonInMiddleOfAminoAcidSequenceThrowsArgumentException()
        {
            var aminoAcids = new List<AminoAcidName> {AminoAcidName.Alanine, AminoAcidName.StopCodon, AminoAcidName.Serine};
            Assert.That(() => new IntronExonExtractor("AAATTTAAATTTAAAAAAA", aminoAcids, MinimumExonLength), Throws.ArgumentException);
        }

        [Test]
        public void SingleAminoAcidIsAligned()
        {
            var nucleotides = "ATGTAG";
            var aminoAcids = "M";
            var sut = new IntronExonExtractor(nucleotides, aminoAcids, MinimumExonLength);

            var actual = sut.Extract();

            Assert.That(actual.Exons.Count, Is.EqualTo(1));
            Assert.That(actual.Introns.Count, Is.EqualTo(0));
            var exon = actual.Exons.Single();
            Assert.That(exon.StartNucelotideIndex, Is.EqualTo(0));
            Assert.That(exon.EndNucleotideIndex, Is.EqualTo(5));
            Assert.That(exon.AminoAcids.Select(x => x.AminoAcid), Is.EqualTo(new[] { AminoAcidName.Methionine, AminoAcidName.StopCodon }));
        }

        [Test]
        public void SingleAminoAcidWithStartAndEndIntronsIsAligned()
        {
            var nucleotides = "CATGTAGT";
            var aminoAcids = "M";
            var sut = new IntronExonExtractor(nucleotides, aminoAcids, MinimumExonLength);

            var actual = sut.Extract();

            Assert.That(actual.Exons.Count, Is.EqualTo(1));
            Assert.That(actual.Introns.Count, Is.EqualTo(2));
            var exon = actual.Exons.Single();
            Assert.That(exon.StartNucelotideIndex, Is.EqualTo(1));
            Assert.That(exon.EndNucleotideIndex, Is.EqualTo(6));
            Assert.That(exon.AminoAcids.Select(x => x.AminoAcid), Is.EqualTo(new[] { AminoAcidName.Methionine, AminoAcidName.StopCodon }));

            var startIntron = actual.Introns[0];
            Assert.That(startIntron.StartNucelotideIndex, Is.EqualTo(0));
            Assert.That(startIntron.Nucelotides, Is.EqualTo(new[] { Nucleotide.C }));

            var endIntron = actual.Introns[1];
            Assert.That(endIntron.StartNucelotideIndex, Is.EqualTo(7));
            Assert.That(endIntron.Nucelotides, Is.EqualTo(new[] { Nucleotide.T }));
        }

        [Test]
        public void ThrowsExceptionIfMatchingNotSuccessful()
        {
            var nucleotides = "CTGGGAATGAATGGGGGTCATTGTTCCAAGTAGGGTAAAAA";
            var aminoAcids = "KLYNGGI";
            var sut = new IntronExonExtractor(nucleotides, aminoAcids, MinimumExonLength);

            Assert.That(() => sut.Extract(), Throws.Exception);
        }

        [Test]
        public void LongestPossibleExonIsCreated()
        {
            var nucleotides = "AAACTTTATAATGGTAAGCTGTACAACGGCACGATATAG";
            var aminoAcids = "KLYNGTI";
            var sut = new IntronExonExtractor(nucleotides, aminoAcids, MinimumExonLength);

            var actual = sut.Extract();

            Assert.That(actual.Exons.Count, Is.EqualTo(1));
            Assert.That(actual.Introns.Count, Is.EqualTo(1));
            var exon = actual.Exons.Single();
            Assert.That(exon.StartNucelotideIndex, Is.EqualTo(15));
            Assert.That(exon.EndNucleotideIndex, Is.EqualTo(15 + 3 * (aminoAcids.Length + 1) - 1)); // +1 for stop codon

            var intron = actual.Introns[0];
            Assert.That(intron.StartNucelotideIndex, Is.EqualTo(0));
        }
    }
}
