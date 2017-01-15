using System;

namespace Commons
{
    public class Point3D
    {
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public double DistanceTo(Point3D otherPoint)
        {
            var diffX = X - otherPoint.X;
            var diffY = Y - otherPoint.Y;
            var diffZ = Z - otherPoint.Z;
            var distance = Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
            return distance;
        }
    }
}