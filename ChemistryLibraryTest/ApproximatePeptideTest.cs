using ChemistryLibrary.Builders;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Physics;
using NUnit.Framework;

namespace ChemistryLibraryTest
{
    [TestFixture]
    public class ApproximatePeptideTest
    {
        [Test]
        public void ApproximatePeptidePositioningTest()
        {
            var peptide = ApproximatePeptideBuilder.FromSequence(new string('A', 3));
            Assert.That(peptide.AminoAcids.Count, Is.EqualTo(3));
        }

        [Test]
        public void AminoAcidPositioningTest()
        {
            var aminoAcid1 = new ApproximatedAminoAcid(AminoAcidName.Glycine)
            {
                OmegaAngle = 0.To(Unit.Degree),
                PhiAngle = 0.To(Unit.Degree),
                PsiAngle = -120.To(Unit.Degree)
            };
            var aminoAcid2 = new ApproximatedAminoAcid(AminoAcidName.Glycine)
            {
                OmegaAngle = 10.To(Unit.Degree),
                PhiAngle = -70.To(Unit.Degree),
                PsiAngle = 0.To(Unit.Degree)
            };
            var peptide = new ApproximatePeptide(new [] { aminoAcid1, aminoAcid2});
            var angles = AminoAcidAngleMeasurer.MeasureAngles(peptide);
            var aminoAcid1Angles = angles[aminoAcid1];
            var aminoAcid2Angles = angles[aminoAcid2];

            Assert.That(aminoAcid1Angles.Psi.In(Unit.Radians), Is.EqualTo(aminoAcid1.PsiAngle.In(Unit.Radians)).Within(1e-9));
            Assert.That(aminoAcid2Angles.Omega.In(Unit.Radians), Is.EqualTo(aminoAcid2.OmegaAngle.In(Unit.Radians)).Within(1e-9));
            Assert.That(aminoAcid2Angles.Phi.In(Unit.Radians), Is.EqualTo(aminoAcid2.PhiAngle.In(Unit.Radians)).Within(1e-9));
        }
    }
}
