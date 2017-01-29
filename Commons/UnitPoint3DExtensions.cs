namespace Commons
{
    public static class UnitPoint3DExtensions
    {
        public static UnitPoint3D To(this Point3D point, Unit unit)
        {
            return point.To(SIPrefix.None, unit);
        }
        public static UnitPoint3D To(this Point3D point, CompoundUnit unit)
        {
            return new UnitPoint3D(unit, point.X, point.Y, point.Z);
        }
        public static UnitPoint3D To(this Point3D point, SIPrefix siPrefix, Unit unit)
        {
            return new UnitPoint3D(siPrefix, unit, point.X, point.Y, point.Z);
        }

        public static Point3D In(this UnitPoint3D unitPoint, Unit targetUnit)
        {
            return unitPoint.In(SIPrefix.None, targetUnit);
        }
        public static Point3D In(this UnitPoint3D unitPoint, CompoundUnit targetUnit)
        {
            return new Point3D(
                unitPoint.X.In(targetUnit),
                unitPoint.Y.In(targetUnit),
                unitPoint.Z.In(targetUnit));
        }
        public static Point3D In(this UnitPoint3D unitPoint, SIPrefix targetSIPrefix, Unit targetUnit)
        {
            return new Point3D(
                unitPoint.X.In(targetSIPrefix, targetUnit),
                unitPoint.Y.In(targetSIPrefix, targetUnit),
                unitPoint.Z.In(targetSIPrefix, targetUnit));
        }
        public static UnitValue DistanceTo(this UnitPoint3D unitPoint1, UnitPoint3D unitPoint2)
        {
            var commonUnit = unitPoint1.X.Unit;
            var point1 = unitPoint1.In(commonUnit);
            var point2 = unitPoint2.In(commonUnit);

            return point1.DistanceTo(point2).To(commonUnit);
        }

        public static UnitVector3D VectorTo(this UnitPoint3D unitPoint1, UnitPoint3D unitPoint2)
        {
            var commonUnit = unitPoint1.X.Unit;
            var point1 = unitPoint1.In(commonUnit);
            var point2 = unitPoint2.In(commonUnit);

            return point1.VectorTo(point2).To(commonUnit);
        }
    }
}