using System.Globalization;

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

        public static Point3D operator +(Point3D point, Vector3D vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        public static Point3D operator +(Point3D point, Point3D vector)
        {
            return new Point3D(point.X + vector.X, point.Y + vector.Y, point.Z + vector.Z);
        }

        public static Point3D operator *(double scalar, Point3D point)
        {
            return new Point3D(scalar*point.X, scalar*point.Y, scalar*point.Z);
        }

        public override string ToString()
        {
            return $"{X.ToString("F6", CultureInfo.InvariantCulture)};" +
                   $"{Y.ToString("F6", CultureInfo.InvariantCulture)};" +
                   $"{Z.ToString("F6", CultureInfo.InvariantCulture)}";
        }
    }
}