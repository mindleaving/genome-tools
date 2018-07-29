using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Optimization;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Studies
{
    public static class DistanceMapTo3DCalculator
    {
        private const double CutoffDistance = 5000;

        public static IList<Point3D> Calculate(string imagePath)
        {
            var distanceMap = new Image<Gray, byte>(imagePath);
            return Calculate(distanceMap);
        }
        public static IList<Point3D> Calculate(Image<Gray, byte> distanceMap)
        {
            var aminoAcidCount = distanceMap.Height;
            var aminoAcidPositions = new Point3D[aminoAcidCount];

            // We need 3 reference points which will be our coordinate system

            // Reference 1:
            aminoAcidPositions[0] = new Point3D(0, 0, 0);
            var referenceIndices = new List<int> {0};

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
            referenceIndices.Add(mostDistantAminoAcidIdx);

            // Reference 3:
            var distanceRef0Ref1 = mostDistantAminoAcid.Distance;
            var aminoAcidDistancesToRef1 = AminoAcidDistancesToIndex(distanceMap, mostDistantAminoAcidIdx);
            var largestTriangleInequalityIdx = -1;
            var largestTriangleInequalityDiff = double.NegativeInfinity;
            for (int aminoAcidIdx = 1; aminoAcidIdx < aminoAcidCount; aminoAcidIdx++)
            {
                if(referenceIndices.Contains(aminoAcidIdx))
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
            var ref2X = Math.Sqrt(distanceRef0Ref2.Square() - triangleHeight.Square());
            var ref2Y = triangleHeight;
            aminoAcidPositions[largestTriangleInequalityIdx] = new Point3D(ref2X, ref2Y, 0);
            var actualDistanceToRef0 = aminoAcidPositions[largestTriangleInequalityIdx].DistanceTo(aminoAcidPositions[referenceIndices[0]]);
            if((actualDistanceToRef0 - distanceRef0Ref2).Abs() > 10)
                throw new Exception();
            referenceIndices.Add(largestTriangleInequalityIdx);

            // Reference 4:
            var aminoAcidDistancesToRef2 = AminoAcidDistancesToIndex(distanceMap, largestTriangleInequalityIdx);
            largestTriangleInequalityIdx = -1;
            largestTriangleInequalityDiff = double.NegativeInfinity;
            for (int aminoAcidIdx = 1; aminoAcidIdx < aminoAcidCount; aminoAcidIdx++)
            {
                if(referenceIndices.Contains(aminoAcidIdx))
                    continue;
                var distanceToRef0 = aminoAcidDistancesToRef0[aminoAcidIdx];
                var distanceToRef1 = aminoAcidDistancesToRef1[aminoAcidIdx];
                var distanceToRef2 = aminoAcidDistancesToRef2[aminoAcidIdx];
                var triangleInequalityDiff01 = distanceToRef0 + distanceToRef1 - distanceRef0Ref1;
                var triangleInequalityDiff02 = distanceToRef0 + distanceToRef2 - distanceRef0Ref2;
                var triangleInequalityDiff12 = distanceToRef1 + distanceToRef2 - distanceRef1Ref2;
                var minTriangleInequalityDiff = new[] { triangleInequalityDiff01, triangleInequalityDiff02, triangleInequalityDiff12}.Min();
                if (minTriangleInequalityDiff > largestTriangleInequalityDiff)
                {
                    largestTriangleInequalityDiff = minTriangleInequalityDiff;
                    largestTriangleInequalityIdx = aminoAcidIdx;
                }
            }
            // Find position from optimization
            var limit = mostDistantAminoAcid.Distance;
            var parameterSetting = new[]
            {
                new ParameterSetting("X", -limit, limit, 1, limit / 2),
                new ParameterSetting("Y", -limit, limit, 1, limit / 2),
                new ParameterSetting("Y", -limit, limit, 1, limit / 2)
            };
            var targetDistances = referenceIndices.ToDictionary(
                idx => idx,
                idx => IntensityToDistance(distanceMap[largestTriangleInequalityIdx, idx].Intensity));

            double CostFunc(double[] parameters)
            {
                double deviationSum = 0;
                var point = new Point3D(parameters[0], parameters[1], parameters[2]);
                foreach (var referenceIndex in referenceIndices)
                {
                    var referencePoint = aminoAcidPositions[referenceIndex];
                    var distanceToReference = point.DistanceTo(referencePoint);
                    var targetDistance = targetDistances[referenceIndex];
                    deviationSum += (distanceToReference - targetDistance).Abs();
                }
                return deviationSum;
            }

            var optimizationResult = ParameterCyclingOptimizer.Optimize(CostFunc, parameterSetting);
            if(optimizationResult.Cost > 10)
                throw new Exception("Reference 4 solution not accurate enough");
            aminoAcidPositions[largestTriangleInequalityIdx] = new Point3D(
                optimizationResult.Parameters[0],
                optimizationResult.Parameters[1],
                optimizationResult.Parameters[2]);
            referenceIndices.Add(largestTriangleInequalityIdx);

            var skippedIndices = new List<int>();
            var referencePoints = referenceIndices
                .Select(referenceIndex => aminoAcidPositions[referenceIndex])
                .ToList();
            foreach (var aminoAcidIdx in Enumerable.Range(0, aminoAcidCount).Except(referenceIndices))
            {
                var distancesToRefs = referenceIndices
                    .Select(referenceIndex =>
                        IntensityToDistance(distanceMap[referenceIndex, aminoAcidIdx].Intensity))
                    .ToList();
                if (distancesToRefs.Any(distance => distance >= CutoffDistance))
                {
                    skippedIndices.Add(aminoAcidIdx);
                    continue;
                }
                aminoAcidPositions[aminoAcidIdx] = DeterminePositionFromReference(distancesToRefs, referencePoints);
            }

            var iterations = 0;
            while (skippedIndices.Count > 0 && iterations < 10)
            {
                iterations++;
                referencePoints.Clear();
                var referencePointCandidateIndices = aminoAcidPositions
                    .Select((position, idx) => new {Index = idx, IsKnown = position != null})
                    .Where(x => x.IsKnown)
                    .Select(x => x.Index)
                    .ToList();
                for (int idx = 0; idx < 4; idx++)
                {
                    var candidateIdx = StaticRandom.Rng.Next(referencePointCandidateIndices.Count);
                    referencePoints.Add(aminoAcidPositions[candidateIdx]);
                    referencePointCandidateIndices.RemoveAt(candidateIdx);
                }
                var skippedIndicesCopy = skippedIndices.ToList();
                skippedIndices.Clear();
                foreach (var aminoAcidIdx in skippedIndicesCopy)
                {
                    var distancesToRefs = referenceIndices
                        .Select(referenceIndex =>
                            IntensityToDistance(distanceMap[referenceIndex, aminoAcidIdx].Intensity))
                        .ToList();
                    if (distancesToRefs.Any(distance => distance >= CutoffDistance))
                    {
                        skippedIndices.Add(aminoAcidIdx);
                        continue;
                    }
                    aminoAcidPositions[aminoAcidIdx] = DeterminePositionFromReference(distancesToRefs, referencePoints);
                }
            }

            return aminoAcidPositions;
        }

        private static Point3D DeterminePositionFromReference(List<double> distances, List<Point3D> referencePoints)
        {
            if(referencePoints.Count < 4)
                throw new ArgumentException("At least 4 reference points are needed");
            var A = new double[referencePoints.Count-1,3];
            var b = new double[referencePoints.Count - 1];
            for (int idx = 0; idx < referencePoints.Count-1; idx++)
            {
                A[idx, 0] = referencePoints[idx + 1].X - referencePoints[0].X;
                A[idx, 1] = referencePoints[idx + 1].Y - referencePoints[0].Y;
                A[idx, 2] = referencePoints[idx + 1].Z - referencePoints[0].Z;
                var distanceBetweenReferences = referencePoints[idx + 1].DistanceTo(referencePoints[0]);
                b[idx] = 0.5*(distances[0].Square() - distances[idx+1].Square() + distanceBetweenReferences.Square());
            }
            var beta = A.Transpose().Multiply(A).Inverse().Multiply(A.Transpose().Multiply(b.ConvertToMatrix())).Vectorize();
            return new Point3D(
                beta[0]+referencePoints[0].X, 
                beta[1]+referencePoints[0].Y, 
                beta[2]+referencePoints[0].Z);
        }

        private static List<double> AminoAcidDistancesToIndex(Image<Gray, byte> distanceMap, int aminoAcidIdx)
        {
            var aminoAcidCount = distanceMap.Height;
            return Enumerable.Range(0, aminoAcidCount)
                .Select(idx => IntensityToDistance(distanceMap[aminoAcidIdx, idx].Intensity))
                .ToList();
        }

        private static double CalculateTriangleArea(double a, double b, double c)
        {
            var p = (a + b + c) / 2;
            return Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        }

        private static double IntensityToDistance(double intensity)
        {
            return CutoffDistance * (intensity / 255.0);
        }
    }
}
