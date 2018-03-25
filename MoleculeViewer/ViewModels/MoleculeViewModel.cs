using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ChemistryLibrary.Objects;
using Commons;
using Commons.DataProcessing;
using Commons.Mathematics;
using Commons.Wpf;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace MoleculeViewer.ViewModels
{
    public class MoleculeViewModel : Viewport3DBase
    {
        private const double DimensionScaling = 1e12;

        private List<Atom> atoms;
        private ModelVisual3D moleculeModel;
        private readonly TrapLatch updateModelLatch;
        private int modelUpdatedCount;
        private ICommand resetViewCommand;
        // ###############################
        // NOTE ON UNITS: ALL POSITIONS AND SIZES ARE IN PICOMETER
        // ###############################

        public ModelVisual3D MoleculeModel
        {
            get { return moleculeModel; }
            private set
            {
                moleculeModel = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler MoleculeHasChanged;

        public int ModelUpdatedCount
        {
            get { return modelUpdatedCount; }
            private set
            {
                modelUpdatedCount = value; 
                OnPropertyChanged();
            }
        }

        public ICommand ResetViewCommand
        {
            get { return resetViewCommand; }
            set { resetViewCommand = value; OnPropertyChanged(); }
        }

        public MoleculeViewModel()
        {
            updateModelLatch = new TrapLatch(
                obj => { },
                obj => Application.Current.Dispatcher.BeginInvoke(new Action(() => DrawAtomsImpl((IEnumerable<Atom>)obj))),
                obj => { })
            {
                State = TrapLatch.LatchState.Trapping
            };
            ResetViewCommand = new RelayCommand(ResetView);
        }

        public void ResetView()
        {
            if(atoms != null)
                SetCameraPosition();
        }

        public void DrawAtoms(IEnumerable<Atom> newAtoms)
        {
            updateModelLatch.Invoke(newAtoms);
        }

        private void DrawAtomsImpl(IEnumerable<Atom> newAtoms)
        {
            var atomCountBefore = atoms?.Count ?? 0;
            atoms = newAtoms.ToList();
            var atomCountAfter = atoms.Count;
            BuildModel();
            if(atomCountBefore != atomCountAfter)
                SetCameraPosition();
        }

        private void BuildModel()
        {
            if (atoms == null || !atoms.Any())
                return;
            var modelContent = new Model3DGroup();
            foreach (var atom in atoms.Where(atom => atom.Position != null))
            {
                var atomSphere = ConstructAtom(atom);
                modelContent.Children.Add(atomSphere);
            }
            var lightSource1 = new DirectionalLight(Colors.Gray, new Vector3D(1, -1, 1));
            var lightSource2 = new AmbientLight(Colors.Gray);
            modelContent.Children.Add(lightSource1);
            modelContent.Children.Add(lightSource2);
            MoleculeModel = new ModelVisual3D {Content = modelContent};
            ModelUpdatedCount++;
            MoleculeHasChanged?.Invoke(this, EventArgs.Empty);
            Task.Delay(100).ContinueWith(task => updateModelLatch.State = TrapLatch.LatchState.Trapping);
        }

        private void SetCameraPosition()
        {
            if(atoms == null || !atoms.Any())
                return;
            var atomPositions = atoms.Select(atom => DimensionScaling*atom.Position).ToList();
            var xMinMax = new MinMaxMean(atomPositions.Select(p => p.X));
            var yMinMax = new MinMaxMean(atomPositions.Select(p => p.Y));
            var zMinMax = new MinMaxMean(atomPositions.Select(p => p.Z));
            var moleculeCenter = new Point3D(xMinMax.Mean, yMinMax.Mean, zMinMax.Mean);
            var position = new Point3D(moleculeCenter.X, moleculeCenter.Y, moleculeCenter.Z - xMinMax.Span);
            Camera.Position = position;
            Camera.LookDirection = new Vector3D(
                moleculeCenter.X - position.X,
                moleculeCenter.Y - position.Y,
                moleculeCenter.Z - position.Z);
            Camera.UpDirection = new Vector3D(0, 1, 0);
            Camera.FieldOfView = 60;
        }

        private GeometryModel3D ConstructAtom(Atom atom)
        {
            var atomColor = DetermineAtomColor(atom.Element, atom.IsBackbone);
            var material = new DiffuseMaterial(new SolidColorBrush(atomColor));
            var sphereMesh = BuildAtomSphere(atom);
            return new GeometryModel3D(sphereMesh, material);
        }

        private MeshGeometry3D BuildAtomSphere(Atom atom)
        {
            var center = DimensionScaling*atom.Position;
            var centerX = center.X;
            var centerY = center.Y;
            var centerZ = center.Z;
            var radius = DimensionScaling*atom.Radius.Value;

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

        private static Color DetermineAtomColor(ElementName atomElement, bool atomIsBackbone)
        {
            var color = Colors.LightGray;
            switch (atomElement)
            {
                case ElementName.Hydrogen:
                    color = Colors.White;
                    break;
                case ElementName.Carbon:
                    color = Color.FromRgb(0x33, 0x33, 0x33);
                    break;
                case ElementName.Nitrogen:
                    color = Colors.Blue;
                    break;
                case ElementName.Oxygen:
                    color = Colors.Red;
                    break;
                case ElementName.Fluorine:
                case ElementName.Chlorine:
                    color = Colors.LawnGreen;
                    break;
                case ElementName.Sulfur:
                    color = Colors.Yellow;
                    break;
            }
            if (!atomIsBackbone)
                color.A = 0x55;
            return color;
        }
    }
}