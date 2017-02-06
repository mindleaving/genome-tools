namespace Commons
{
    public static class UnitVector3DExtensions
    {
        public static UnitVector3D To(this Vector3D point, Unit unit)
        {
            return point.To(SIPrefix.None, unit);
        }
        public static UnitVector3D To(this Vector3D point, CompoundUnit unit)
        {
            return new UnitVector3D(unit, point.X, point.Y, point.Z);
        }
        public static UnitVector3D To(this Vector3D point, SIPrefix siPrefix, Unit unit)
        {
            return new UnitVector3D(siPrefix, unit, point.X, point.Y, point.Z);
        }

        public static Vector3D In(this UnitVector3D unitPoint, Unit targetUnit)
        {
            return unitPoint.In(SIPrefix.None, targetUnit);
        }
        public static Vector3D In(this UnitVector3D unitPoint, CompoundUnit targetUnit)
        {
            return new Vector3D(
                unitPoint.X.In(targetUnit),
                unitPoint.Y.In(targetUnit),
                unitPoint.Z.In(targetUnit));
        }
        public static Vector3D In(this UnitVector3D unitPoint, SIPrefix targetSIPrefix, Unit targetUnit)
        {
            return new Vector3D(
                unitPoint.X.In(targetSIPrefix, targetUnit),
                unitPoint.Y.In(targetSIPrefix, targetUnit),
                unitPoint.Z.In(targetSIPrefix, targetUnit));
        }
        public static UnitValue Magnitude(this UnitVector3D a)
        {
            var commonUnit = a.X.Unit;
            return a.In(commonUnit).Magnitude().To(commonUnit);
        }
    }
}