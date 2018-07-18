using Domain;
using NUnit.Framework;

namespace Studies
{
    [TestFixture]
    public class PeptideFrequencyStudy
    {
        [Test]
        public void FindCommonPeptideCombinations()
        {
            var outputDirectory = @"G:\Projects\HumanGenome\sequenceFrequencies\AllGRCh38Peptides";
            var peptides = PeptideFileReader.ReadPeptides(@"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.pep.all.fa");
            PeptideFrequencyAnalysis.Analyze(peptides, outputDirectory);
        }
    }
}
