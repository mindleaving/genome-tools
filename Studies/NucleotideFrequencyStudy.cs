using Domain;
using NUnit.Framework;

namespace Studies
{
    [TestFixture]
    public class NucleotideFrequencyStudy
    {
        [Test]
        public void FindCommonNucelotideSequences()
        {
            var fileName = @"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.dna.primary_assembly.fa";
            WholeGenomeFrequencyAnalysis.Analyze(fileName);
        }
    }
}
