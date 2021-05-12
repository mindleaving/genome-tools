using Commons.Extensions;
using Commons.Physics;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public static class DihedralAngleCalculator
    {
        public static UnitValue Calculate(UnitPoint3D point1, 
            UnitPoint3D point2, 
            UnitPoint3D point3, 
            UnitPoint3D point4)
        {
            var vector1 = point1.VectorTo(point2).Normalize().ToVector3D();
            var vector2 = point2.VectorTo(point3).Normalize().ToVector3D();
            var vector3 = point3.VectorTo(point4).Normalize().ToVector3D();

            var plane12Normal = vector1.CrossProduct(vector2);
            var plane23Normal = vector2.CrossProduct(vector3);

            var angle = plane12Normal.AngleWith(plane23Normal);
            var sign = vector3.DotProduct(plane12Normal) > 0 ? 1 : -1;
            return sign*angle;
        }
    }
}