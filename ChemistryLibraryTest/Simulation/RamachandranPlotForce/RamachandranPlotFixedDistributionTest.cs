using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;
using GenomeTools.ChemistryLibrary.Simulation.RamachadranPlotForce;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Simulation.RamachandranPlotForce
{
    [TestFixture]
    public class RamachandranPlotFixedDistributionTest
    {
        [Test]
        [TestCase(40, -10, -10, 0)]
        [TestCase(20, -10, 10, 0)]
        [TestCase(30, 0, 0, -10)]
        [TestCase(30, -20, 0, 10)]
        [TestCase(-170, -10, -160, 0)]
        [TestCase(30, 175, 0, 175)]
        public void PhiPsiVectorPointingTowardTarget(double phi, double psi, 
            double expectedPhiComponent, double expectedPsiComponent)
        {
            var targetPhi = 30.To(Unit.Degree);
            var targetPsi = -10.To(Unit.Degree);

            var sut = new RamachandranPlotFixedDistribution(AminoAcidName.Alanine, 
                new UnitPoint2D(targetPhi, targetPsi));
            var actual = sut.GetPhiPsiVector(phi.To(Unit.Degree), psi.To(Unit.Degree));

            Assert.That(actual.In(Unit.Degree).X, Is.EqualTo(expectedPhiComponent).Within(1));
            Assert.That(actual.In(Unit.Degree).Y, Is.EqualTo(expectedPsiComponent).Within(1));
        }
    }
}
