using System.IO;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Fasta;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Fasta
{
    public class FastaIndexReaderTest
    {
        [Test]
        public void CanReadFastaIndex()
        {
            var input = "NC_000001.11\t248956422\t1024\t80\t81\r\n" 
                        + "NT_187361.1\t175055\t252068864\t80\t81\r\n"
                        + "NT_187362.1\t32032\t252246016\t80\t81";
            var inputStream = new MemoryStream(Encoding.ASCII.GetBytes(input));
            var sut = new FastaIndexReader();

            var actual = sut.ReadIndex(inputStream);

            Assert.That(actual.Count, Is.EqualTo(3));
            Assert.That(actual[0].SequenceName, Is.EqualTo("NC_000001.11"));
            Assert.That(actual[1].LineWidth, Is.EqualTo(81));
            Assert.That(actual[2].FirstBaseOffset, Is.EqualTo(252246016));
        }
    }
}
