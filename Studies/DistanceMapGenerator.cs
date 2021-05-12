using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace GenomeTools.Studies
{
    public static class DistanceMapGenerator
    {
        public static readonly double CutoffDistance = 5000; // pm

        public static Image<Gray, byte> Generate(List<UnitPoint3D> aminoAcidPositions)
        {
            var aminoAcidPositionsInPicoMeters = aminoAcidPositions.Select(p => p.In(SIPrefix.Pico, Unit.Meter)).ToList();
            return Generate(aminoAcidPositionsInPicoMeters);
        }
        public static Image<Gray, byte> Generate(List<Point3D> positionsInPicoMeters)
        {
            var distanceMap = new Image<Gray, byte>(new Size(positionsInPicoMeters.Count, positionsInPicoMeters.Count));
            for (int idx1 = 0; idx1 < positionsInPicoMeters.Count; idx1++)
            {
                for (int idx2 = idx1; idx2 < positionsInPicoMeters.Count; idx2++)
                {
                    var distance = positionsInPicoMeters[idx1].DistanceTo(positionsInPicoMeters[idx2]);
                    var intensity = Math.Min(byte.MaxValue, Math.Round(255*distance / CutoffDistance));
                    distanceMap[idx1, idx2] = new Gray(intensity);
                    distanceMap[idx2, idx1] = new Gray(intensity);
                }
            }
            return distanceMap;
        }
    }
}
