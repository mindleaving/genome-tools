using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram.Models
{
    public class BaseSubstitutionMatrixTest
    {
        [Test]
        public void SubstitutionAsExpected()
        {
            var substitutionMatrixBytes = new byte[]
            {
                0b00011011, // A->C: 00, A->G: 01, A->T: 10, A->N: 11
                0b11100100,
                0b01111000,
                0b10010011,
                0b00110110
            };
            var sut = new BaseSubstitutionMatrix(substitutionMatrixBytes);

            Assert.That(sut.Substitute('A', 0b01), Is.EqualTo('G'));
            Assert.That(sut.Substitute('A', 0b11), Is.EqualTo('N'));
            Assert.That(sut.Substitute('C', 0b11), Is.EqualTo('A'));
            Assert.That(sut.Substitute('G', 0b10), Is.EqualTo('T'));
            Assert.That(sut.Substitute('T', 0b01), Is.EqualTo('C'));
            Assert.That(sut.Substitute('N', 0b01), Is.EqualTo('G'));
        }
    }
}
