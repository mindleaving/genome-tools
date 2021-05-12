using Commons.Extensions;
using Commons.Mathematics;

namespace GenomeTools.Studies
{
    public class LinearTransformation3D
    {
        public LinearTransformation3D(Vector3D translation, Matrix3X3 rotation)
        {
            Translation = translation;
            Rotation = rotation;
        }

        public Vector3D Translation { get; }
        public Matrix3X3 Rotation { get; }

        public Point3D Apply(Point3D point)
        {
            var pointArray = point.ToVector3D().Data;
            var rotatedPoint = new Vector(Rotation.Data.Multiply(pointArray.ConvertToMatrix()).Vectorize());
            return (rotatedPoint + Translation).ToVector3D().ToPoint3D();
        }
    }
}