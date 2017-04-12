using ChemistryLibrary;
using NUnit.Framework;

namespace ChemistryLibraryTest
{
    [TestFixture]
    public class ApproximatePeptideTest
    {
        [Test]
        public void ApproximatePeptidePositioningTest()
        {
            var peptide = new ApproximatePeptide(new string('A', 1480));
            Assert.That(peptide.AminoAcids.Count, Is.EqualTo(3));
        }
    }
}
