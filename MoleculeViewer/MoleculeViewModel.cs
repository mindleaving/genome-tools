using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ChemistryLibrary;
using Commons;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace MoleculeViewer
{
    public class MoleculeViewModel : Viewport3DBase
    {
        // ###############################
        // NOTE ON UNITS: ALL POSITIONS AND SIZES ARE IN PICOMETER
        // ###############################

        public ModelVisual3D MoleculeModel { get; private set; }

        public MoleculeViewModel(Molecule molecule)
        {
            var modelContent = new Model3DGroup();
            foreach (var atom in molecule.Atoms)
            {
                var atomSphere = ConstructAtom(atom);
                modelContent.Children.Add(atomSphere);
            }
            var lightSource1 = new DirectionalLight(Colors.Gray, new Vector3D(1, -1, 1));
            var lightSource2 = new AmbientLight(Colors.Gray);
            modelContent.Children.Add(lightSource1);
            modelContent.Children.Add(lightSource2);
            MoleculeModel = new ModelVisual3D {Content = modelContent};

            var moleculeCenter = new Point3D(
                molecule.Atoms.Average(atom => atom.Position.X),
                molecule.Atoms.Average(atom => atom.Position.Y),
                molecule.Atoms.Average(atom => atom.Position.Z));
            var position = new Point3D(moleculeCenter.X, moleculeCenter.Y - 100, moleculeCenter.Z - 1000);
            var lookDirection = new Vector3D(
                moleculeCenter.X - position.X, 
                moleculeCenter.Y - position.Y,
                moleculeCenter.Z - position.Z);
            var upDirection = new Vector3D(0, 1, 0);
            var fieldOfView = 60;
            Camera = new PerspectiveCamera(position, lookDirection, upDirection, fieldOfView);
        }

        private GeometryModel3D ConstructAtom(Atom atom)
        {
            var atomColor = DetermineAtomColor(atom.Element);
            var material = new DiffuseMaterial(new SolidColorBrush(atomColor));
            var sphereMesh = BuildAtomSphere(atom);
            return new GeometryModel3D(sphereMesh, material);
        }

        private MeshGeometry3D BuildAtomSphere(Atom atom)
        {
            var center = atom.Position;
            var centerX = center.X.To(center.SIPrefix, center.Unit).In(SIPrefix.Pico, Unit.Meter);
            var centerY = center.Y.To(center.SIPrefix, center.Unit).In(SIPrefix.Pico, Unit.Meter);
            var centerZ = center.Z.To(center.SIPrefix, center.Unit).In(SIPrefix.Pico, Unit.Meter);
            var radius = atom.Radius.In(SIPrefix.Pico, Unit.Meter);

            return BuildSphere(new Point3D(centerX, centerY, centerZ), radius);
        }

        // Add a sphere.
        // Copied from http://csharphelper.com/blog/2015/04/draw-spheres-using-wpf-and-c/
        private MeshGeometry3D BuildSphere(Point3D center, double radius)
        {
            const int LongitudinalPointCount = 8;
            const int LatitudinalPointCount = 6; // Cannot be below 3

            var mesh = new MeshGeometry3D();
            const double dphi = Math.PI/LatitudinalPointCount;
            const double dtheta = 2*Math.PI/LongitudinalPointCount;

            double phi0 = 0;
            var y0 = radius*Math.Cos(phi0);
            var r0 = radius*Math.Sin(phi0);
            for (int i = 0; i < LatitudinalPointCount; i++)
            {
                var phi1 = phi0 + dphi;
                var y1 = radius*Math.Cos(phi1);
                var r1 = radius*Math.Sin(phi1);

                // Point ptAB has phi value A and theta value B.
                // For example, pt01 has phi = phi0 and theta = theta1.
                // Find the points with theta = theta0.
                double theta0 = 0;
                var pt00 = new Point3D(
                    center.X + r0*Math.Cos(theta0),
                    center.Y + y0,
                    center.Z + r0*Math.Sin(theta0));
                var pt10 = new Point3D(
                    center.X + r1*Math.Cos(theta0),
                    center.Y + y1,
                    center.Z + r1*Math.Sin(theta0));
                for (int j = 0; j < LongitudinalPointCount; j++)
                {
                    // Find the points with theta = theta1.
                    var theta1 = theta0 + dtheta;
                    var pt01 = new Point3D(
                        center.X + r0*Math.Cos(theta1),
                        center.Y + y0,
                        center.Z + r0*Math.Sin(theta1));
                    var pt11 = new Point3D(
                        center.X + r1*Math.Cos(theta1),
                        center.Y + y1,
                        center.Z + r1*Math.Sin(theta1));

                    // Create the triangles.
                    AddTriangle(mesh, pt00, pt11, pt10);
                    AddTriangle(mesh, pt00, pt01, pt11);

                    // Move to the next value of theta.
                    theta0 = theta1;
                    pt00 = pt01;
                    pt10 = pt11;
                }

                // Move to the next value of phi.
                phi0 = phi1;
                y0 = y1;
                r0 = r1;
            }
            return mesh;
        }

        // Add a triangle to the indicated mesh.
        private void AddTriangle(MeshGeometry3D mesh,
            Point3D point1, Point3D point2, Point3D point3)
        {
            // Get the points' indices.
            var index1 = AddPoint(mesh.Positions, point1);
            var index2 = AddPoint(mesh.Positions, point2);
            var index3 = AddPoint(mesh.Positions, point3);

            // Create the triangle.
            mesh.TriangleIndices.Add(index1);
            mesh.TriangleIndices.Add(index2);
            mesh.TriangleIndices.Add(index3);
        }

        private int AddPoint(Point3DCollection meshPositions, Point3D point)
        {
            meshPositions.Add(point);
            return meshPositions.Count - 1;
        }

        private static Color DetermineAtomColor(ElementName atomElement)
        {
            switch (atomElement)
            {
                case ElementName.Hydrogen:
                    return Colors.White;
                case ElementName.Carbon:
                    return Color.FromRgb(0x33, 0x33, 0x33);
                case ElementName.Nitrogen:
                    return Colors.Blue;
                case ElementName.Oxygen:
                    return Colors.Red;
                case ElementName.Fluorine:
                case ElementName.Chlorine:
                    return Colors.LawnGreen;
                case ElementName.Sulfur:
                    return Colors.Yellow;
                default:
                    return Colors.LightGray;
            }
        }
    }
}