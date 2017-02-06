using System;

namespace Commons
{
    public static class Vector2DExtensions
    {
        public static Vector2D Divide(this Vector2D vector, double scalar)
        {
            return new Vector2D(vector.X / scalar, vector.Y / scalar);
        }

        public static double Magnitude(this Vector2D a)
        {
            return Math.Sqrt(a.X * a.X + a.Y * a.Y);
        }

        public static double Determinant(this Vector2D a, Vector2D b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
    public static class Vector3DExtensions
    {
        /// <summary>
        /// Divide the vector by its length to produce a unit vector
        /// </summary>
        /// <returns>The resulting unit vector</returns>
        public static Vector3D Normalize(this Vector3D a)
        {
            var magnitude = a.Magnitude();
            return a.Divide(magnitude);
        }

        public static Vector3D ProjectOnto(this Vector3D v1, Vector3D v2)
        {
            return v1.DotProduct(v2)*v2;
        }

        public static Vector3D Divide(this Vector3D vector, double scalar)
        {
            return new Vector3D(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
        }

        public static Vector3D Multiply(this Vector3D vector, double scalar)
        {
            return new Vector3D(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
        }

        public static double DotProduct(this Vector3D v1, Vector3D v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static Vector3D CrossProduct(this Vector3D a, Vector3D b)
        {
            return new Vector3D(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        public static double Magnitude(this Vector3D a)
        {
            return Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        }

        public static double AngleWith(this Vector3D v1, Vector3D v2)
        {
            var normalizedV1 = v1.Normalize();
            var normalizedV2 = v2.Normalize();
            return Math.Acos(normalizedV1.DotProduct(normalizedV2));
        }

        public static Point3D ToPoint3D(this Vector3D v)
        {
            return new Point3D(v.X, v.Y, v.Z);
        }
    }
}