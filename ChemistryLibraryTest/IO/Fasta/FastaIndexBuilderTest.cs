using System.IO;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Fasta;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Fasta
{
    public class FastaIndexBuilderTest
    {
        [Test]
        public void CanCreateIndex()
        {
            var input = ">chr1 Some comment\n" 
                        + "AGGGCAT\n"
                        + "TTGCAAN\n"
                        + "TAAG\n"
                        + ">chr2\n" 
                        + "CCGATTT\n"
                        + "TAAGCCA\n";
            var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));
            var sut = new FastaIndexBuilder();

            var actual = sut.BuildIndex(inputStream);
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[0].SequenceName, Is.EqualTo("chr1"));
            Assert.That(actual[0].FirstBaseOffset, Is.EqualTo(19));
            Assert.That(actual[0].Length, Is.EqualTo(18));
            Assert.That(actual[0].BasesPerLine, Is.EqualTo(7));
            Assert.That(actual[0].LineWidth, Is.EqualTo(8));

            Assert.That(actual[1].SequenceName, Is.EqualTo("chr2"));
            Assert.That(actual[1].FirstBaseOffset, Is.EqualTo(46));
            Assert.That(actual[1].Length, Is.EqualTo(14));
            Assert.That(actual[1].BasesPerLine, Is.EqualTo(7));
            Assert.That(actual[1].LineWidth, Is.EqualTo(8));
        }
    }
}
