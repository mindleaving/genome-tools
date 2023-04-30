using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Vcf;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Vcf
{
    public class VcfExtensionsTest
    {
        private const string PersonId = "Test";

        [Test]
        [TestCase("1")]
        [TestCase("1|1")]
        [TestCase("1|0")]
        [TestCase("0|1")]
        [TestCase("1/1")]
        [TestCase("1/0")]
        [TestCase("0/1")]
        public void HasVariantTrue(string input)
        {
            var variant = new VcfVariantEntry(
                PersonId,
                "chr1",
                0,
                "",
                "A",
                new[] { "T" },
                "Q",
                VcfFilterResult.Success(),
                null,
                new Dictionary<string, string>
                {
                    { PersonId, input }
                });

            Assert.That(variant.HasPersonVariant(PersonId), Is.True);
        }

        [Test]
        [TestCase("0")]
        [TestCase("0|0")]
        [TestCase("0/0")]
        public void HasVariantFalse(string input)
        {
            var variant = new VcfVariantEntry(
                PersonId,
                "chr1",
                0,
                "",
                "A",
                new[] { "T" },
                "Q",
                VcfFilterResult.Success(),
                null,
                new Dictionary<string, string>
                {
                    { PersonId, input }
                });

            Assert.That(variant.HasPersonVariant(PersonId), Is.False);
        }
    }
}
