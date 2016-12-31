namespace Commons
{
    public class Vector3D : Vector
    {
        public double X
        {
            get { return Data[0]; }
            set { Data[0] = value; }
        }
        public double Y
        {
            get { return Data[1]; }
            set { Data[1] = value; }
        }
        public double Z
        {
            get { return Data[2]; }
            set { Data[2] = value; }
        }

        public Vector3D() : base(3) { }
        public Vector3D(params double[] data) : base(3, data) { }
        public Vector3D(double x, double y, double z) : base(3, x, y, z) { }

        /// <summary>
        /// Calculate cross product.
        /// </summary>
        public static Vector3D operator *(Vector3D u, Vector3D v)
        {
            // Cross product
            return new Vector3D(
                u.Y * v.Z - u.Z * u.Y,
                u.Z * v.X - u.X * v.Z,
                u.X * v.Y - u.Y * v.X
                );
        }
    }
}