using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO
{
    public class GenomeReadTest
    {
        [Test]
        public void SequenceIsBuildFromFeatures()
        {
            var readFeatures = new List<GenomeReadFeature>
            {
                new(GenomeSequencePartType.Bases, 0, "ABC".ToCharArray()),
                new(GenomeSequencePartType.Deletion, 2, deletionLength: 2),
                new(GenomeSequencePartType.BaseWithQualityScore, 3, new List<char> { 'G'}, new List<char> { '('}),
                new(GenomeSequencePartType.Insertion, 4, "GGTC".ToCharArray(), "....".ToCharArray()),
                new(GenomeSequencePartType.BaseWithQualityScore, 8, new List<char> { 'T'}, new List<char> { 'B'})
            };
            var sut = GenomeRead.MappedRead(-1, 0, 9, readFeatures, 0);
            
            Assert.That(sut.GetSequence(), Is.EqualTo("ABCGGGTCT"));
            Assert.That(sut.GetReferenceAlignedSequence(), Is.EqualTo("ABC--GT"));
            Assert.That(sut.GetQualityScores(), Is.EqualTo("\0\0\0(....B"));
            Assert.That(sut.GetReferenceQualityScores(), Is.EqualTo("\0\0\0\0\0(B"));
            Assert.That(sut.GetBaseAtPosition(1), Is.EqualTo('B'));
            Assert.That(sut.GetBaseAtPosition(6), Is.EqualTo('T'));
        }
    }
}
