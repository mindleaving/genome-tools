using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons
{
    public static class Point3DExtensions
    {
        public static double DistanceTo(this Point3D point1, Point3D point2)
        {
            var diffX = point1.X - point2.X;
            var diffY = point1.Y - point2.Y;
            var diffZ = point1.Z - point2.Z;
            var distance = Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
            return distance;
        }

        public static Vector3D VectorTo(this Point3D point1, Point3D point2)
        {
            var diffX = point2.X - point1.X;
            var diffY = point2.Y - point1.Y;
            var diffZ = point2.Z - point1.Z;
            return new Vector3D(diffX, diffY, diffZ);
        }

        public static Vector3D ToVector3D(this Point3D point3D)
        {
            return new Vector3D(point3D.X, point3D.Y, point3D.Z);
        }

        public static Point3D Average(this IEnumerable<Point3D> points)
        {
            var pointList = points.ToList();
            return new Point3D(
                pointList.Average(p => p.X),
                pointList.Average(p => p.Y),
                pointList.Average(p => p.Z));
        }
    }
}