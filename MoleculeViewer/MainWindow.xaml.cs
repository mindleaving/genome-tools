using System.Linq;
using System.Windows;
using ChemistryLibrary;
using Commons;

namespace MoleculeViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", 
            typeof(MoleculeViewModel), typeof(MainWindow), new PropertyMetadata(default(MoleculeViewModel)));

        public MainWindow()
        {
            InitializeComponent();

            var molecule = new Molecule();
            var oxygen1 = molecule.AddAtom(Atom.FromStableIsotope(ElementName.Oxygen));
            molecule.AddAtom(Atom.FromStableIsotope(ElementName.Iodine), oxygen1, BondMultiplicity.Double);
            //molecule.PositionAtoms();
            molecule.Atoms.First().Position = new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 0, 0, 0);
            molecule.Atoms.Last().Position = new UnitPoint3D(SIPrefix.Pico, 
                Unit.Meter, 0.9*(molecule.Atoms.First().Radius + molecule.Atoms.Last().Radius).In(SIPrefix.Pico, Unit.Meter), 0, 0);

            ViewModel = new MoleculeViewModel(molecule);
            Viewport3D.Children.Add(ViewModel.MoleculeModel);
            Viewport3D.Camera = ViewModel.Camera;
            Viewport3D.UpdateLayout();
        }

        public MoleculeViewModel ViewModel
        {
            get { return (MoleculeViewModel) GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
    }
}
