namespace Commons
{
    public class UnitVector3D
    {
        public UnitValue X { get; set; }
        public UnitValue Y { get; set; }
        public UnitValue Z { get; set; }

        public UnitVector3D(UnitValue x, UnitValue y, UnitValue z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public UnitVector3D(SIPrefix prefix, Unit unit, double x, double y, double z)
        {
            X = x.To(prefix, unit);
            Y = y.To(prefix, unit);
            Z = z.To(prefix, unit);
        }

        public UnitVector3D(CompoundUnit unit, double x, double y, double z)
        {
            X = x.To(unit);
            Y = y.To(unit);
            Z = z.To(unit);
        }

        public UnitVector3D(Unit unit, double x, double y, double z)
            : this(SIPrefix.None, unit, x, y, z)
        {
        }

        public static UnitVector3D operator +(UnitVector3D v1, UnitVector3D v2)
        {
            var x = v1.X + v2.X;
            var y = v1.Y + v2.Y;
            var z = v1.Z + v2.Z;
            return new UnitVector3D(x, y, z);
        }
        public static UnitVector3D operator -(UnitVector3D v1, UnitVector3D v2)
        {
            var x = v1.X - v2.X;
            var y = v1.Y - v2.Y;
            var z = v1.Z - v2.Z;
            return new UnitVector3D(x, y, z);
        }
        public static UnitVector3D operator -(UnitVector3D v)
        {
            return -1.0 * v;
        }
        public static UnitVector3D operator *(UnitValue scalar, UnitVector3D v)
        {
            var x = scalar * v.X;
            var y = scalar * v.Y;
            var z = scalar * v.Z;
            return new UnitVector3D(x, y, z);
        }
        public static UnitVector3D operator *(int scalar, UnitVector3D v)
        {
            return (double)scalar * v;
        }
        public static UnitVector3D operator *(UnitVector3D v, double scalar)
        {
            return scalar * v;
        }
        public static UnitVector3D operator *(UnitVector3D v, UnitValue scalar)
        {
            return scalar * v;
        }
        public static UnitVector3D operator *(UnitVector3D v, int scalar)
        {
            return scalar * v;
        }
        public static UnitVector3D operator *(double scalar, UnitVector3D v)
        {
            var x = scalar * v.X;
            var y = scalar * v.Y;
            var z = scalar * v.Z;
            return new UnitVector3D(x, y, z);
        }
        public static UnitVector3D operator /(UnitVector3D v, UnitValue scalar)
        {
            var x = v.X / scalar;
            var y = v.Y / scalar;
            var z = v.Z / scalar;
            return new UnitVector3D(x, y, z);
        }

        public override string ToString()
        {
            return $"{X};{Y};{Z}";
        }
    }
}