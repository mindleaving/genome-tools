using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public static class NBodyForceCalculator
    {
        public static Dictionary<Point3D, Vector3D> Calculate(List<Point3D> points, Func<Point3D, Point3D, Vector3D> forceFunc)
        {
            var forceLookup = points.ToDictionary(p => p, p => new Vector3D());
            for (int idx1 = 0; idx1 < points.Count; idx1++)
            {
                var point1 = points[idx1];
                for (int idx2 = idx1+1; idx2 < points.Count; idx2++)
                {
                    var point2 = points[idx2];
                    var force = forceFunc(point1, point2);

                    forceLookup[point1] += force;
                    forceLookup[point2] += -force;
                }
            }
            return forceLookup;
        }
    }
}
