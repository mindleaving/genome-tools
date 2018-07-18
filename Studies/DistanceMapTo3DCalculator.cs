using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Studies
{
    public class DistanceMapTo3DCalculator
    {
        private const double CutoffDistance = 5000;

        public IList<Point3D> Calculate(string imagePath)
        {
            var distanceMap = new Image<Gray, byte>(imagePath);
            var aminoAcidCount = distanceMap.Height;
            var aminoAcidPositions = new Point3D[aminoAcidCount];

            // We need 3 reference points which will be our coordinate system

            // Reference 1:
            aminoAcidPositions[0] = new Point3D(0, 0, 0);
            var referencePositions = new List<ReferencePosition> {new ReferencePosition(aminoAcidPositions[0], 0)};

            // Reference 2:
            var aminoAcidDistancesToRef0 = AminoAcidDistancesToIndex(distanceMap, 0);
            var mostDistantAminoAcid = aminoAcidDistancesToRef0.Select((distance, idx) => new
                {
                    Index = idx,
                    Distance = distance
                })
                .Where(x => x.Distance < CutoffDistance)
                .MaximumItem(x => x.Distance);
            var mostDistantAminoAcidIdx = mostDistantAminoAcid.Index;
            aminoAcidPositions[mostDistantAminoAcidIdx] = new Point3D(mostDistantAminoAcid.Distance, 0, 0);
            referencePositions.Add(new ReferencePosition(aminoAcidPositions[mostDistantAminoAcidIdx], mostDistantAminoAcidIdx));

            // Reference 3:
            var distanceRef0Ref1 = mostDistantAminoAcid.Distance;
            var aminoAcidDistancesToRef1 = AminoAcidDistancesToIndex(distanceMap, mostDistantAminoAcidIdx);
            var largestTriangleInequalityIdx = -1;
            var largestTriangleInequalityDiff = double.NegativeInfinity;
            for (int aminoAcidIdx = 1; aminoAcidIdx < aminoAcidCount; aminoAcidIdx++)
            {
                if(aminoAcidIdx == mostDistantAminoAcidIdx)
                    continue;
                var distanceToRef0 = aminoAcidDistancesToRef0[aminoAcidIdx];
                var distanceToRef1 = aminoAcidDistancesToRef1[aminoAcidIdx];
                var triangleInequalityDiff = distanceToRef0 + distanceToRef1 - distanceRef0Ref1;
                if (triangleInequalityDiff > largestTriangleInequalityDiff)
                {
                    largestTriangleInequalityDiff = triangleInequalityDiff;
                    largestTriangleInequalityIdx = aminoAcidIdx;
                }
            }
            var distanceRef0Ref2 = aminoAcidDistancesToRef0[largestTriangleInequalityIdx];
            var distanceRef1Ref2 = aminoAcidDistancesToRef1[largestTriangleInequalityIdx];
            var triangleArea = CalculateTriangleArea(distanceRef0Ref1, distanceRef0Ref2, distanceRef1Ref2);
            var triangleHeight = 2 * triangleArea / distanceRef0Ref1;
            var ref2X = Math.Sqrt(distanceRef0Ref2 * distanceRef0Ref2 - triangleHeight * triangleHeight);
            var ref2Y = triangleHeight;
            aminoAcidPositions[largestTriangleInequalityIdx] = new Point3D(ref2X, ref2Y, 0);
            referencePositions.Add(new ReferencePosition(aminoAcidPositions[largestTriangleInequalityIdx], largestTriangleInequalityIdx));

            var aminoAcidDistancesToRef2 = AminoAcidDistancesToIndex(distanceMap, largestTriangleInequalityIdx);

            var skippedIndices = new List<int>();
            foreach (var aminoAcidIdx in Enumerable.Range(0, aminoAcidCount).Except(referencePositions.Select(x => x.AminoAcidIndex)))
            {
                
            }


            return aminoAcidPositions;
        }

        private static List<double> AminoAcidDistancesToIndex(Image<Gray, byte> distanceMap, int aminoAcidIdx)
        {
            var aminoAcidCount = distanceMap.Height;
            return Enumerable.Range(0, aminoAcidCount)
                .Select(idx => IntensityToDistance(distanceMap[aminoAcidIdx, idx].Intensity))
                .ToList();
        }

        private double CalculateTriangleArea(double a, double b, double c)
        {
            var p = (a + b + c) / 2;
            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        private static double IntensityToDistance(double intensity)
        {
            return CutoffDistance * (intensity / 255.0);
        }

        private class ReferencePosition
        {
            public ReferencePosition(Point3D position, int aminoAcidIndex)
            {
                Position = position;
                AminoAcidIndex = aminoAcidIndex;
            }

            public Point3D Position { get; }
            public int AminoAcidIndex { get; }
        }
    }
}
