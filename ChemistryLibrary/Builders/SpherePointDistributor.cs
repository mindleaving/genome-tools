using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using Commons.Extensions;
using Commons.Mathematics;
using GenomeTools.ChemistryLibrary.Simulation;

namespace GenomeTools.ChemistryLibrary.Builders
{
    public static class SpherePointDistributor
    {
        public static List<Point3D> EvenlyDistributePointsOnSphere(double radius, int pointCount, IEnumerable<Vector3D> existingPoints)
        {
            //File.Delete(@"F:\HumanGenome\SpherePointDistribution_debug.csv");

            var scaledExistingPoints = existingPoints
                .Select(v => (v.Magnitude()/radius)*v)
                .Select(v => v.ToPoint3D())
                .ToList();
            
            // Generate points
            var points = new List<Point3D>();
            for (int pointIdx = 0; pointIdx < pointCount; pointIdx++)
            {
                var theta = 2*Math.PI*StaticRandom.Rng.NextDouble();
                var phi = Math.PI * StaticRandom.Rng.NextDouble();
                var point = new Point3D(
                    radius * Math.Cos(theta) * Math.Sin(phi),
                    radius * Math.Sin(theta) * Math.Sin(phi),
                    radius * Math.Cos(phi));
                points.Add(point);
            }

            Func<Point3D, Point3D, Vector3D> repulsiveForce = (point1, point2) =>
            {
                var distance = point1.DistanceTo(point2);
                return -(1.0/distance)*point1.VectorTo(point2);
            };
            const int iterationPerPoint = 25;
            var maxIterations = iterationPerPoint*pointCount;
            var iteration = 0;
            var frozenPoints = new List<Point3D>();
            do
            {
                var forces = NBodyForceCalculator.Calculate(
                    points.Concat(scaledExistingPoints).ToList(), repulsiveForce);
                var maxDisplacement = double.NegativeInfinity;
                var displacementLimit = 0.3 * radius / (0.8*iteration+1);

                // Points are gradually frozen to avoid points chasing each other
                var pointsToMove = points.Except(frozenPoints).ToList();
                if(!pointsToMove.Any())
                    break;
                var firstMovablePoint = pointsToMove.First();
                var distanceToOtherPoints = pointsToMove
                    .Except(new[] {firstMovablePoint})
                    .Select(p => firstMovablePoint.DistanceTo(p))
                    .OrderBy(x => x)
                    .ToList();
                foreach (var point in pointsToMove)
                {
                    var force = forces[point];
                    var normalForce = force.ProjectOnto(point.ToVector3D().Normalize());
                    var tangentialForce = force - normalForce;
                    var displacement = tangentialForce.Magnitude() > displacementLimit
                        ? displacementLimit / tangentialForce.Magnitude() * tangentialForce
                        : tangentialForce;
                    if (displacement.Magnitude() > maxDisplacement)
                        maxDisplacement = displacement.Magnitude();
                    var displacedPoint = point + displacement.ToVector3D();
                    var distanceToCenter = displacedPoint.ToVector3D().Magnitude();
                    var scaling = radius/distanceToCenter;
                    point.X = scaling * displacedPoint.X;
                    point.Y = scaling * displacedPoint.Y;
                    point.Z = scaling * displacedPoint.Z;
                }
                if(pointsToMove.Count == 1 && maxDisplacement < 0.2*displacementLimit)
                    break;

                if (pointsToMove.Count > 1)
                {
                    // Freeze point
                    var averagePoint = points.Average();
                    var newDistanceToOtherPoints = pointsToMove
                        .Except(new[] { firstMovablePoint })
                        .Select(p => firstMovablePoint.DistanceTo(p))
                        .OrderBy(x => x)
                        .ToList();
                    var distanceDifference = distanceToOtherPoints
                        .Select((d1, idx) => Math.Abs(d1 - newDistanceToOtherPoints[idx]))
                        .Max();
                    if ((averagePoint.DistanceTo(new Point3D(0, 0, 0)) < 0.005 * radius
                        && distanceDifference < 0.005 * radius)
                        || iteration > iterationPerPoint * (frozenPoints.Count+1))
                        frozenPoints.Add(pointsToMove.First());
                }
                iteration++;
                //File.AppendAllText(@"F:\HumanGenome\SpherePointDistribution_debug.csv",
                //    points.Select(p => p.ToString()).Aggregate((a, b) => a + ";" + b) + Environment.NewLine);
            } while (iteration < maxIterations);
            return points;
        }
    }
}