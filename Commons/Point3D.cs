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
    }

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
}