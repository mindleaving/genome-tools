using System;

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
    }
}