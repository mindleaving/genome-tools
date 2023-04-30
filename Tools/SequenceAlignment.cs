using GenomeTools.ChemistryLibrary.Measurements;
using NUnit.Framework;

namespace GenomeTools.Tools
{
    public class SequenceAlignment
    {
        [Test]
        [TestCase(
            "CATCAGCACCGTTGTCATCAGTACCACCACTAGCATCATGGCAGCTAGCATGCTTGACCACCTTCTGTGTGCCTGGCACCATGCAAAGCTCTTTGCACAC", 
            "CATCAGCACCGTGGTGTAATCAGAGTGTGGCCAACATCATCGCGCTTTGGCAGGCTAGCCTGAGAACCACCCTTGGGATTTTACATTGTTTCAAAGAAGC"
        )]
        public void Align(string sequence1, string sequence2)
        {
            var aligner = new SmithWatermanAligner<char>();
            var alignmentResult = aligner.Align(sequence1.ToCharArray(), sequence2.ToCharArray(), (a, b) => a == b);
            
        }
    }
}
