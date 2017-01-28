using ChemistryLibrary;
using NUnit.Framework;

namespace ChemistryLibraryTest
{
    [TestFixture]
    public class OrbitalComparerTest
    {
        [Test]
        [TestCase(1,OrbitalType.s, 2, OrbitalType.s)]
        [TestCase(2, OrbitalType.s, 2, OrbitalType.p)]
        [TestCase(2, OrbitalType.p, 3, OrbitalType.s)]
        [TestCase(3, OrbitalType.s, 3, OrbitalType.p)]
        [TestCase(3, OrbitalType.p, 4, OrbitalType.s)]
        [TestCase(4, OrbitalType.s, 3, OrbitalType.d)]
        [TestCase(3, OrbitalType.d, 4, OrbitalType.p)]
        [TestCase(4, OrbitalType.p, 5, OrbitalType.s)]
        [TestCase(5, OrbitalType.s, 4, OrbitalType.d)]
        [TestCase(6, OrbitalType.s, 4, OrbitalType.f)]
        public void OrderAsExpected(int period1, OrbitalType type1, int period2, OrbitalType type2)
        {
            var sut = OrbitalComparer.Instance;
            var orbital1 = new Orbital(null, period1, type1);
            var orbital2 = new Orbital(null, period2, type2);
            Assert.That(sut.Compare(orbital1, orbital2), Is.LessThan(0));
        }
    }
}
