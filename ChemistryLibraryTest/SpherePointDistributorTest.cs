using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Builders;
using Commons.Mathematics;
using NUnit.Framework;

namespace ChemistryLibraryTest
{
    [TestFixture]
    public class SpherePointDistributorTest
    {
        [Test]
        public void TwoPointsArePositionedOpposite()
        {
            var points = SpherePointDistributor.EvenlyDistributePointsOnSphere(1.0, 2, new List<Vector3D>());
            
            Assert.That(points.Count, Is.EqualTo(2));
            var point1 = points[0];
            var point2 = points[1];
            Assert.That(point1.X, Is.EqualTo(-point2.X).Within(0.01));
            Assert.That(point1.Y, Is.EqualTo(-point2.Y).Within(0.01));
            Assert.That(point1.Z, Is.EqualTo(-point2.Z).Within(0.01));
        }

        [Test]
        public void ThirdPointsPlacedBetweenTwoPoles()
        {
            var points = SpherePointDistributor.EvenlyDistributePointsOnSphere(1.0, 1, new List<Vector3D>
                {
                    new Vector3D(1,0,0),
                    new Vector3D(-1,0,0)
                });

            Assert.That(points.Count, Is.EqualTo(1));
            Assert.That(points.Single().X, Is.EqualTo(0).Within(0.01));
        }
    }
}
