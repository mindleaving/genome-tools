using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoleculeViewer
{
    /// <summary>
    /// Interaction logic for MoleculeView.xaml
    /// </summary>
    public partial class MoleculeView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(MoleculeViewModel), typeof(MoleculeView), new PropertyMetadata(default(MoleculeViewModel)));

        public MoleculeView()
        {
            InitializeComponent();
        }

        public MoleculeViewModel ViewModel
        {
            get { return (MoleculeViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        private bool firstLoad = true;
        private void Carbon3DView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (firstLoad && ViewModel?.MoleculeModel != null)
            {
                try
                {
                    Viewport3D.Children.Add(ViewModel.MoleculeModel);
                    Viewport3D.Camera = ViewModel.Camera;
                    ViewModel.MoleculeHasChanged += (sender1, e1) => UpdateViewport3D();
                    firstLoad = false;
                }
                catch (ArgumentException)
                {
                    // If view is loaded multiple times, the ViewModel.Scene object is alrady bound (to what?)
                    // resulting in an exception.
                }
            }
        }

        private void UpdateViewport3D()
        {
            if (Application.Current.Dispatcher.HasShutdownStarted)
                return;
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(UpdateViewport3D));
                return;
            }
            Viewport3D.Children.Clear();
            Viewport3D.Children.Add(ViewModel.MoleculeModel);
        }

        private void Carbon3DView_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ViewModel.MoveBackForth(e.Delta / 120.0);
            e.Handled = true;
        }

        private Point? lastMousePosition;
        private void Carbon3DView_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed
                && e.RightButton != MouseButtonState.Pressed
                && e.MiddleButton != MouseButtonState.Pressed)
                return;
            var position = e.GetPosition(Viewport3D);
            if (lastMousePosition.HasValue)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    ViewModel.Pan(0.05 * (position.X - lastMousePosition.Value.X), 0.05 * (position.Y - lastMousePosition.Value.Y));
                else if (e.RightButton == MouseButtonState.Pressed)
                    ViewModel.RotateObject(0.05 * (position.X - lastMousePosition.Value.X),
                        0.05 * (position.Y - lastMousePosition.Value.Y),
                        new System.Windows.Media.Media3D.Point3D(0, 0, 0));
                else if (e.MiddleButton == MouseButtonState.Pressed)
                    ViewModel.RotateLookDirection(0.1 * (position.X - lastMousePosition.Value.X), 0.1 * (position.Y - lastMousePosition.Value.Y));
            }
            lastMousePosition = position;
            e.Handled = true;
        }

        private void Carbon3DView_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            lastMousePosition = null;
        }

        private void Carbon3DView_OnMouseEnter(object sender, MouseEventArgs e)
        {
            lastMousePosition = null;
        }
    }
}
