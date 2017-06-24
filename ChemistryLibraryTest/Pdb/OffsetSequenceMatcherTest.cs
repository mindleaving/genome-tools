using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.IO.Pdb;
using NUnit.Framework;

namespace ChemistryLibraryTest.Pdb
{
    [TestFixture]
    public class OffsetSequenceMatcherTest
    {
        [Test]
        public void ThrowsExceptionForEmptySequence()
        {
            Assert.That(() => new OffsetSequenceMatcher<int>(new List<int>(), new List<int> { 1, 2} ), Throws.Exception);
            Assert.That(() => new OffsetSequenceMatcher<int>(new List<int> { 1, 2}, new List<int>()), Throws.Exception);
        }

        [Test]
        public void EqualSequencesAreMatched()
        {
            var sequence1 = new[] {1, 2, 3};
            var sequence2 = new[] { 1, 2, 3 };
            var sut = new OffsetSequenceMatcher<int>(sequence1, sequence2);
            Assert.That(sut.OverlapPercentage, Is.EqualTo(100));
            Assert.That(sut.CombinedSequence, Is.EqualTo(sequence1));
        }

        [Test]
        public void SubsequenceIsMatched()
        {
            var sequenceOffset = 2;
            var sequence1 = new[] {1, 2, 3, 4, 5};
            var sequence2 = sequence1.Skip(sequenceOffset).ToArray();
            var sut = new OffsetSequenceMatcher<int>(sequence1, sequence2);
            Assert.That(sut.OverlapPercentage, Is.EqualTo(100));
            Assert.That(sut.CombinedSequence, Is.EqualTo(sequence1));
            Assert.That(sut.SequenceOffset.Item1, Is.EqualTo(sequenceOffset));
        }

        [Test]
        public void PartiallyOveralappingSequencesAreMatched()
        {
            var sequence1 = new[] {1, 2, 3, 4, 5};
            var sequence2 = new[] {3, 4, 5, 6, 7};
            var sut = new OffsetSequenceMatcher<int>(sequence1, sequence2);
            Assert.That(sut.OverlapPercentage, Is.EqualTo(60));
            Assert.That(sut.CombinedSequence, Is.EqualTo(new[] {1, 2, 3, 4, 5, 6, 7}));
            Assert.That(sut.SequenceOffset.Item1, Is.EqualTo(2));
        }

        [Test]
        public void PartiallyOveralappingSequencesAreMatched2()
        {
            var sequence1 = new[] { 3, 4, 5, 6, 7 };
            var sequence2 = new[] { 1, 2, 3, 4, 5};
            var sut = new OffsetSequenceMatcher<int>(sequence1, sequence2);
            Assert.That(sut.OverlapPercentage, Is.EqualTo(60));
            Assert.That(sut.CombinedSequence, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7 }));
            Assert.That(sut.SequenceOffset.Item2, Is.EqualTo(2));
        }
    }
}
