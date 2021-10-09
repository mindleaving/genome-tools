using GenomeTools.ChemistryLibrary.Extensions;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Extensions
{
    public class ParserHelpersTest
    {
        [Test]
        [TestCase("ID=PGT,Number=1,Type=String,Description=\"Physical phasing haplotype information, describing how the alternate alleles are phased in relation to one another; will always be heterozygous and is not intended to describe called alleles\"")]
        public void CorrectlySplitsString(string str)
        {
            var actual = str.QuoteAwareSplit(',');
            Assert.That(actual.Count, Is.EqualTo(4));
            Assert.That(actual[0], Is.EqualTo("ID=PGT"));
            Assert.That(actual[1], Is.EqualTo("Number=1"));
            Assert.That(actual[2], Is.EqualTo("Type=String"));
            Assert.That(actual[3], Is.EqualTo("Description=\"Physical phasing haplotype information, describing how the alternate alleles are phased in relation to one another; will always be heterozygous and is not intended to describe called alleles\""));
        }
    }
}
