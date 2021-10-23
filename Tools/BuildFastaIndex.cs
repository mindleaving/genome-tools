using GenomeTools.ChemistryLibrary.IO.Fasta;
using NUnit.Framework;

namespace GenomeTools.Tools
{
    public class BuildFastaIndex
    {
        [Test]
        [TestCase(@"F:\datasets\mygenome\hg38.fna")]
        public void BuildIndex(string filePath)
        {
            var indexBuilder = new FastaIndexBuilder();
            indexBuilder.BuildIndexAndWriteToFile(filePath);
        }
    }
}
