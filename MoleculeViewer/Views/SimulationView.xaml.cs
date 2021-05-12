using System.Windows;
using System.Windows.Controls;
using GenomeTools.MoleculeViewer.ViewModels;

namespace GenomeTools.MoleculeViewer.Views
{
    /// <summary>
    /// Interaction logic for SimulationView.xaml
    /// </summary>
    public partial class SimulationView : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(SimulationViewModel), typeof(SimulationView), new PropertyMetadata(default(SimulationViewModel)));

        public SimulationView()
        {
            InitializeComponent();
        }

        public SimulationViewModel ViewModel
        {
            get { return (SimulationViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
