using GenomeTools.ChemistryLibrary.IO.Fasta;
using NUnit.Framework;

namespace GenomeTools.Tools
{
    public class FastaIndexBuilderRunner
    {
        [Test]
        [TestCase(@"F:\datasets\mygenome\references\hg38.fa")]
        public void BuildIndex(string filePath)
        {
            var indexBuilder = new FastaIndexBuilder();
            indexBuilder.BuildIndexAndWriteToFile(filePath);
        }
    }
}
