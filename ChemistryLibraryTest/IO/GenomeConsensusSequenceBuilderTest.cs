using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO
{
    public class GenomeConsensusSequenceBuilderTest
    {
        [Test]
        public void SingleReadIsReturned()
        {
            var sequence = new GenomeSequence("AGTTTCCCGAAT", "Test", 0);
            var read = new List<GenomeRead>
            {
                GenomeRead.MappedRead(-1, 0, sequence.Length, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, sequence.GetSequence().ToCharArray()) }, 0)
            };
            var alignment = new GenomeSequenceAlignment("chr1", 0, 11, sequence, sequence, read);
            var sut = new GenomeConsensusSequenceBuilder();

            var actual = sut.Build(alignment);

            Assert.That(actual.GetSequence(), Is.EqualTo(sequence.GetSequence()));
        }

        [Test]
        public void ConsensusIsFoundForBlockOfReads()
        {
            var sequence = new GenomeSequence("AGTTTCCCGAAT", "Test", 0);
            var otherSequence = new GenomeSequence("AGCGGTTCGCGG", "Other", 0);
            var read = new List<GenomeRead>
            {
                GenomeRead.MappedRead(-1, 0, sequence.Length, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, sequence.GetSequence().ToCharArray()) }, 0),
                GenomeRead.MappedRead(-1, 0, sequence.Length, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, otherSequence.GetSequence().ToCharArray()) }, 0),
                GenomeRead.MappedRead(-1, 0, otherSequence.Length, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, sequence.GetSequence().ToCharArray()) }, 0)
            };
            var alignment = new GenomeSequenceAlignment("chr1", 0, 11, sequence, sequence, read);
            var sut = new GenomeConsensusSequenceBuilder();

            var actual = sut.Build(alignment);

            Assert.That(actual.GetSequence(), Is.EqualTo(sequence.GetSequence()));
        }
    }
}
