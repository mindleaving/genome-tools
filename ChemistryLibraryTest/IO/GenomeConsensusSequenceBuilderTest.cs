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
            var sequence = "AGTTTCCCGAAT";
            var read = new List<GenomeRead>
            {
                GenomeRead.MappedRead(-1, 0, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, sequence.ToCharArray()) }, 0)
            };
            var alignment = new GenomeSequenceAlignment("chr1", 0, 11, sequence, sequence, read);
            var sut = new GenomeConsensusSequenceBuilder();

            var actual = sut.Build(alignment);

            Assert.That(actual.GetSequence(), Is.EqualTo(sequence));
        }

        [Test]
        public void ConsensusIsFoundForBlockOfReads()
        {
            var sequence = "AGTTTCCCGAAT";
            var otherSequence = "AGCGGTTCGCGG";
            var read = new List<GenomeRead>
            {
                GenomeRead.MappedRead(-1, 0, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, sequence.ToCharArray()) }, 0),
                GenomeRead.MappedRead(-1, 0, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, otherSequence.ToCharArray()) }, 0),
                GenomeRead.MappedRead(-1, 0, new List<GenomeReadFeature> { new(GenomeSequencePartType.Bases, 0, sequence.ToCharArray()) }, 0)
            };
            var alignment = new GenomeSequenceAlignment("chr1", 0, 11, sequence, sequence, read);
            var sut = new GenomeConsensusSequenceBuilder();

            var actual = sut.Build(alignment);

            Assert.That(actual.GetSequence(), Is.EqualTo(sequence));
        }
    }
}
