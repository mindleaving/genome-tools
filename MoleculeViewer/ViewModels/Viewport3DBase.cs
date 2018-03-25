using System;
using System.Windows.Media.Media3D;
using Commons.Extensions;
using Point3D = System.Windows.Media.Media3D.Point3D;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace MoleculeViewer.ViewModels
{
    public abstract class Viewport3DBase : ViewModelBase
    {
        private PerspectiveCamera camera = new PerspectiveCamera();

        public PerspectiveCamera Camera
        {
            get { return camera; }
            protected set
            {
                camera = value; 
                OnPropertyChanged();
            }
        }

        public void MoveBackForth(double change)
        {
            const double Scaling = 600.0; // Determined from user experience

            var lookDirection = ToPantoVector3D(Camera.LookDirection);
            var positionChange = lookDirection.Normalize().Multiply(Scaling * change);
            Camera.Position = new Point3D(
                Camera.Position.X + positionChange.X,
                Camera.Position.Y + positionChange.Y,
                Camera.Position.Z + positionChange.Z);
        }
        public void Zoom(int delta)
        {
            if (Camera.FieldOfView > 90 && delta < 0)
                return;
            if (Camera.FieldOfView < 5 && delta > 0)
                return;
            Camera.FieldOfView -= 0.01 * delta;
        }

        public void Pan(double deltaHorizontal, double deltaVertical)
        {
            const double PositionChangeScale = 300.0; // Determined from user experience
            var positionChange = CalculatePositionChange(deltaHorizontal, deltaVertical)
                .Multiply(PositionChangeScale);
            Camera.Position = new Point3D(
                Camera.Position.X + positionChange.X,
                Camera.Position.Y + positionChange.Y,
                Camera.Position.Z + positionChange.Z);
        }

        public void RotateLookDirection(double horizontalRotation, double verticalRotation)
        {
            const double RotationSpeedScale = 0.03; // Determined from user experience
            var lookDirectionChange = CalculatePositionChange(horizontalRotation, verticalRotation)
                .Multiply(RotationSpeedScale);
            Camera.LookDirection = new Vector3D(
                Camera.LookDirection.X + lookDirectionChange.X,
                Camera.LookDirection.Y + lookDirectionChange.Y,
                Camera.LookDirection.Z + lookDirectionChange.Z);
        }

        private Commons.Mathematics.Vector3D CalculatePositionChange(double deltaHorizontal, double deltaVertical)
        {
            var upDirection = ToPantoVector3D(Camera.UpDirection);
            var imagePlaneNormal = ToPantoVector3D(Camera.LookDirection);
            var verticalDirection = (upDirection - upDirection.ProjectOnto(imagePlaneNormal)).Normalize();
            var horizontalDirection = verticalDirection.CrossProduct(imagePlaneNormal).Normalize();
            var positionChange = verticalDirection.Multiply(deltaVertical) + horizontalDirection.Multiply(deltaHorizontal);
            return positionChange;
        }

        private static Commons.Mathematics.Vector3D ToPantoVector3D(Vector3D cameraUpDirection)
        {
            return new Commons.Mathematics.Vector3D(cameraUpDirection.X, cameraUpDirection.Y, cameraUpDirection.Z);
        }

        public void RotateObject(double horizontalRotation, double verticalRotation, Point3D objectCenter)
        {
            const double PositionChangeScale = 100.0; // Determined from user experience

            var lookDirection = ToPantoVector3D(Camera.LookDirection);
            //Point3D rotationPoint;
            //if (Math.Abs(lookDirection.Z) < 1e-6)
            //{
            //    rotationPoint = new Point3D(0, 0, 0);
            //}
            //else
            //{
            //    // Calculate scaling of look direction vector to hit the object plane.
            //    // Object plane is here defined as x-y-plane with z = 0.
            //    var intersectionVectorScaling = -(Camera.Position.Z - objectCenter.Z) / lookDirection.Z;
            //    rotationPoint = new Point3D(
            //        Camera.Position.X + intersectionVectorScaling * lookDirection.X,
            //        Camera.Position.Y + intersectionVectorScaling * lookDirection.Y,
            //        Camera.Position.Z + intersectionVectorScaling * lookDirection.Z);
            //}
            var lookDirectionMagnitude = lookDirection.Magnitude();
            var rotationPointScaling = 1000/lookDirectionMagnitude;
            var rotationPoint = new Point3D(
                Camera.Position.X + rotationPointScaling*lookDirection.X,
                Camera.Position.Y + rotationPointScaling*lookDirection.Y,
                Camera.Position.Z + rotationPointScaling*lookDirection.Z);
            var distanceToRotationPoint = ToPantoVector3D(rotationPoint - Camera.Position).Magnitude();
            // Move camera up and sideways in current image plane
            var positionChange = CalculatePositionChange(horizontalRotation, verticalRotation)
                .Multiply(PositionChangeScale);
            var pannedPosition = new Point3D(
                Camera.Position.X + positionChange.X,
                Camera.Position.Y + positionChange.Y,
                Camera.Position.Z + positionChange.Z);

            // Point camera at rotation point
            var newLookDirection = new Commons.Mathematics.Vector3D(
                    rotationPoint.X - pannedPosition.X,
                    rotationPoint.Y - pannedPosition.Y,
                    rotationPoint.Z - pannedPosition.Z)
                .Normalize();
            // Do not transform if new look direction almost straight up/down
            if (Math.Abs(newLookDirection.Y) / newLookDirection.Magnitude() > 0.9) // Found from user experience
                return;
            Camera.LookDirection = new Vector3D(newLookDirection.X, newLookDirection.Y, newLookDirection.Z);

            // Ensure that distance to rotation point is preserved by moving camera towards it
            var newDistanceToRotationPoint = ToPantoVector3D(rotationPoint - pannedPosition).Magnitude();
            var distancePreservedPosition = new Point3D(
                pannedPosition.X + (newDistanceToRotationPoint - distanceToRotationPoint) * newLookDirection.X,
                pannedPosition.Y + (newDistanceToRotationPoint - distanceToRotationPoint) * newLookDirection.Y,
                pannedPosition.Z + (newDistanceToRotationPoint - distanceToRotationPoint) * newLookDirection.Z);
            Camera.Position = distancePreservedPosition;
        }
    }
}