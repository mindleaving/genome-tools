using System.Windows;
using ChemistryLibrary;

namespace MoleculeViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty MoleculeViewModelProperty = DependencyProperty.Register("MoleculeViewModel",
            typeof(MoleculeViewModel), typeof(MainWindow), new PropertyMetadata(default(MoleculeViewModel)));
        public static readonly DependencyProperty SimulationViewModelProperty = DependencyProperty.Register("SimulationViewModel", 
            typeof(SimulationViewModel), typeof(MainWindow), new PropertyMetadata(default(SimulationViewModel)));

        public MainWindow()
        {
            InitializeComponent();

            //var water = MoleculeLibrary.H2O;
            //water.PositionAtoms();
            var aminoAcid = AminoAcidLibrary.Proline
                .Add(AminoAcidLibrary.Tyrosine)
                .Add(AminoAcidLibrary.Alanine)
                .Add(AminoAcidLibrary.Glutamine)
                .Add(AminoAcidLibrary.Glutamine)
                .Add(AminoAcidLibrary.Lysine)
                .Molecule;
            aminoAcid.PositionAtoms();

            MoleculeViewModel = new MoleculeViewModel(aminoAcid);
            SimulationViewModel = new SimulationViewModel(MoleculeViewModel);

            SimulationViewModel.RunSimulation();
        }

        public SimulationViewModel SimulationViewModel
        {
            get { return (SimulationViewModel)GetValue(SimulationViewModelProperty); }
            set { SetValue(SimulationViewModelProperty, value); }
        }
        public MoleculeViewModel MoleculeViewModel
        {
            get { return (MoleculeViewModel) GetValue(MoleculeViewModelProperty); }
            set { SetValue(MoleculeViewModelProperty, value); }
        }
    }
}
