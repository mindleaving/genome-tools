using System;

namespace Commons
{
    public class UnitPoint3D
    {
        public UnitValue X { get; set; }
        public UnitValue Y { get; set; }
        public UnitValue Z { get; set; }


        public UnitPoint3D(UnitValue x, UnitValue y, UnitValue z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public UnitPoint3D(SIPrefix prefix, Unit unit, double x, double y, double z)
        {
            X = x.To(prefix, unit);
            Y = y.To(prefix, unit);
            Z = z.To(prefix, unit);
        }

        public UnitPoint3D(CompoundUnit unit, double x, double y, double z)
        {
            X = x.To(unit);
            Y = y.To(unit);
            Z = z.To(unit);
        }
        public UnitPoint3D(Unit unit, double x, double y, double z)
            : this(SIPrefix.None, unit, x, y, z)
        {
        }

        public static UnitPoint3D operator +(UnitPoint3D point1, UnitPoint3D point2)
        {
            var x = point1.X + point2.X;
            var y = point1.Y + point2.Y;
            var z = point1.Z + point2.Z;
            return new UnitPoint3D(x, y, z);
        }
        public static UnitPoint3D operator -(UnitPoint3D point1, UnitPoint3D point2)
        {
            var x = point1.X - point2.X;
            var y = point1.Y - point2.Y;
            var z = point1.Z - point2.Z;
            return new UnitPoint3D(x, y, z);
        }
        public static UnitPoint3D operator +(UnitPoint3D point1, UnitVector3D v)
        {
            var x = point1.X + v.X;
            var y = point1.Y + v.Y;
            var z = point1.Z + v.Z;
            return new UnitPoint3D(x, y, z);
        }
        public static UnitPoint3D operator -(UnitPoint3D point1, UnitVector3D v)
        {
            var x = point1.X - v.X;
            var y = point1.Y - v.Y;
            var z = point1.Z - v.Z;
            return new UnitPoint3D(x, y, z);
        }
        public static UnitPoint3D operator *(double scalar, UnitPoint3D point)
        {
            var x = scalar * point.X;
            var y = scalar * point.Y;
            var z = scalar * point.Z;
            return new UnitPoint3D(x, y, z);
        }
        public static UnitPoint3D operator *(UnitValue scalar, UnitPoint3D point)
        {
            var x = scalar * point.X;
            var y = scalar * point.Y;
            var z = scalar * point.Z;
            return new UnitPoint3D(x, y, z);
        }

        public override string ToString()
        {
            return $"{X};{Y};{Z}";
        }
    }
}