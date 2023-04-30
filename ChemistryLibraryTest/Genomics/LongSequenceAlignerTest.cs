using System.Linq;
using Commons;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.Genomics.Alignment;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Genomics
{
    public class LongSequenceAlignerTest
    {
        [Test]
        public void IdenticalShortSequencesReturnOneMatchedRegion()
        {
            var reference = "ABCDEF";
            var sequenceToBeAligned = reference;
            var sut = new LongSequenceAligner();

            var actual = sut.Align(reference, sequenceToBeAligned);

            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Type, Is.EqualTo(AlignmentRegionType.Match));
            var matchedAlignmentRegion = (MatchedAlignmentRegion)actual[0];
            Assert.That(matchedAlignmentRegion.ReferenceStartIndex, Is.EqualTo(0));
            Assert.That(matchedAlignmentRegion.AlignedSequenceStartIndex, Is.EqualTo(0));
            Assert.That(matchedAlignmentRegion.AlignmentLength, Is.EqualTo(sequenceToBeAligned.Length));
        }

        [Test]
        public void IdenticalLongSequencesReturnOneMatchedRegion()
        {
            var reference = new string(Enumerable.Range(0, 50).Select(_ => (char)StaticRandom.Rng.Next('A', 'Z'+1)).ToArray());
            var sequenceToBeAligned = reference;
            var sut = new LongSequenceAligner();

            var actual = sut.Align(reference, sequenceToBeAligned);

            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Type, Is.EqualTo(AlignmentRegionType.Match));
            var matchedAlignmentRegion = (MatchedAlignmentRegion)actual[0];
            Assert.That(matchedAlignmentRegion.ReferenceStartIndex, Is.EqualTo(0));
            Assert.That(matchedAlignmentRegion.AlignedSequenceStartIndex, Is.EqualTo(0));
            Assert.That(matchedAlignmentRegion.AlignmentLength, Is.EqualTo(sequenceToBeAligned.Length));
        }
    }
}
