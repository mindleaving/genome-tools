namespace Commons
{
    public class UnitPoint3D : Point3D
    {
        public SIPrefix SIPrefix { get; }
        public Unit Unit { get; }

        public UnitPoint3D(SIPrefix prefix, Unit unit, double x, double y, double z)
            : base(x, y, z)
        {
            SIPrefix = prefix;
            Unit = unit;
        }

        public UnitPoint3D(Unit unit, double x, double y, double z) 
            : base(x, y, z)
        {
            SIPrefix = SIPrefix.None;
            Unit = unit;
        }
    }

    public static class UnitPoint3DExtensions
    {
        public static UnitPoint3D To(this Point3D point, Unit unit)
        {
            return point.To(SIPrefix.None, unit);
        }
        public static UnitPoint3D To(this Point3D point, SIPrefix siPrefix, Unit unit)
        {
            return new UnitPoint3D(siPrefix, unit, point.X, point.Y, point.Z);
        }
    }
}