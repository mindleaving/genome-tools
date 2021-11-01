using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO
{
    public class GenomeReadFormatterTest
    {
        [Test]
        public void SimpleReadHasSameReadAndAlignedSequence()
        {
            var readFeatures = new List<GenomeReadFeature>
            {
                new(GenomeSequencePartType.Bases, 0, "AAGGCCTT".ToCharArray())
            };
            var readLength = readFeatures[0].Sequence.Count;
            var sut = new GenomeReadFormatter();

            var readSequence = sut.GetReadSequence(readFeatures, readLength);
            var alignedSequence = sut.GetReferenceAlignedSequence(readFeatures, readLength);

            Assert.That(readSequence, Is.EqualTo(readFeatures[0].Sequence));
            Assert.That(alignedSequence, Is.EqualTo(readSequence));
        }

        [Test]
        public void DeletionIsAddedInAlignedSequence()
        {
            var readFeatures = new List<GenomeReadFeature>
            {
                new(GenomeSequencePartType.Bases, 0, "AAGG".ToCharArray()),
                new(GenomeSequencePartType.Deletion, 4, deletionLength: 2),
                new(GenomeSequencePartType.Bases, 4, "CCTT".ToCharArray()),
            };
            var readLength = 8;
            var sut = new GenomeReadFormatter();

            var readSequence = sut.GetReadSequence(readFeatures, readLength);
            var alignedSequence = sut.GetReferenceAlignedSequence(readFeatures, readLength);

            Assert.That(readSequence, Is.EqualTo("AAGGCCTT"));
            Assert.That(alignedSequence, Is.EqualTo("AAGG--CCTT"));
        }

        [Test]
        public void InsertionIsRemovedInAlignedSequence()
        {
            var readFeatures = new List<GenomeReadFeature>
            {
                new(GenomeSequencePartType.Bases, 0, "AAGG".ToCharArray()),
                new(GenomeSequencePartType.Insertion, 4, "NN".ToCharArray()),
                new(GenomeSequencePartType.Bases, 6, "CCTT".ToCharArray()),
            };
            var readLength = 10;
            var sut = new GenomeReadFormatter();

            var readSequence = sut.GetReadSequence(readFeatures, readLength);
            var alignedSequence = sut.GetReferenceAlignedSequence(readFeatures, readLength);

            Assert.That(readSequence, Is.EqualTo("AAGGNNCCTT"));
            Assert.That(alignedSequence, Is.EqualTo("AAGGCCTT"));
        }
    }
}
