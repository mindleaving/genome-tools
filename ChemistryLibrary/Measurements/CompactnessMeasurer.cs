using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Objects;
using Commons;
using MIConvexHull;

namespace ChemistryLibrary.Measurements
{
    /// <summary>
    /// Measures compactness of peptide chain using convex hull
    /// </summary>
    public static class CompactnessMeasurer
    {
        public static double Measure(ApproximatePeptide peptide)
        {
            var aminoAcidPositions = peptide.AminoAcids
                .SelectMany(aa => new[] {aa.NitrogenPosition, aa.CarbonAlphaPosition, aa.CarbonPosition})
                .ToList();
            var vertices = ToVertices(aminoAcidPositions);
            var convexHull = ConvexHull.Create(vertices);
            var volume = CalculateVolume(convexHull);
            return volume;
        }

        private static double CalculateVolume(ConvexHull<Vertex3D, DefaultConvexFace<Vertex3D>> convexHull)
        {
            const double oneThird = 1.0/3.0;
            var volume = 0.0;
            var origin = convexHull.Points.First().Point;
            foreach (var face in convexHull.Faces)
            {
                var faceOrigin = face.Vertices[1].Point;
                var faceVector1 = (face.Vertices[1].Point - face.Vertices[0].Point).ToVector3D();
                var faceVector2 = (face.Vertices[2].Point - face.Vertices[1].Point).ToVector3D();
                var faceArea = 0.5*faceVector1.CrossProduct(faceVector2).Magnitude();
                var blockHeight = origin.DistanceFromPlane(faceOrigin, faceVector1, faceVector2);
                var blockVolume = oneThird*faceArea*blockHeight;
                volume += blockVolume;
            }
            return volume;
        }

        private class Vertex3D : IVertex
        {
            public Vertex3D(double x, double y, double z)
            {
                Position = new[] {x, y, z};
                Point = new Point3D(x, y, z);
            }

            public double[] Position { get; }
            public Point3D Point { get; }
        }
        private static IList<Vertex3D> ToVertices(List<UnitPoint3D> aminoAcidPositions)
        {
            return aminoAcidPositions
                .Select(aaPosition => aaPosition.In(SIPrefix.Pico, Unit.Meter))
                .Select(aaPosition => new Vertex3D(aaPosition.X, aaPosition.Y, aaPosition.Z))
                .ToList();
        }
    }
}
