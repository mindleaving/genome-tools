using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using ChemistryLibrary.Builders;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.IO;
using ChemistryLibrary.Objects;
using ChemistryLibrary.Simulation;
using Commons;
using MoleculeViewer.ViewModels;

namespace MoleculeViewer.Views
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

            //var filename = @"G:\Projects\HumanGenome\Protein-PDBs\5hhe.pdb";
            var filename = @"G:\Projects\HumanGenome\AminoseqFiles\AAA.aminoseq";
            ApproximatePeptide approximatePeptide;
            if (filename.ToLowerInvariant().EndsWith(".pdb"))
            {
                var peptide = PeptideLoader.Load(filename);
                peptide.Molecule.MarkBackbone(peptide.MoleculeReference);
                peptide.Molecule.PositionAtoms(peptide.MoleculeReference.FirstAtomId, peptide.MoleculeReference.LastAtomId);
                approximatePeptide = ApproximatePeptideBuilder.FromPeptide(peptide);
            }
            else
            {
                approximatePeptide = ApproximatePeptideBuilder.FromSequence(File.ReadAllLines(filename).Aggregate((a,b) => a + b));
                approximatePeptide.UpdatePositions();
            }

            var simulationSettings = new ApproximatePeptideSimulationSettings
            {
                TimeStep = 2.To(SIPrefix.Femto, Unit.Second),
                SimulationTime = 10.To(SIPrefix.Nano, Unit.Second),
                ResetAtomVelocityAfterEachTimestep = false
            };
            var ramachadranDataDirectory = @"G:\Projects\HumanGenome\ramachadranDistributions";
            var simulationRunner = ApproximatePeptideFoldingSimulatorFactory.Create(
                approximatePeptide, simulationSettings, ramachadranDataDirectory);
            //var simulationRunner = new MoleculeDynamicsSimulator(peptide.Molecule, new List<CustomAtomForce>(),
            //    new MoleculeDynamicsSimulationSettings
            //    {
            //        TimeStep = 2.To(SIPrefix.Femto, Unit.Second),
            //        SimulationTime = 10.To(SIPrefix.Pico, Unit.Second)
            //    });
            MoleculeViewModel = new MoleculeViewModel();
            MoleculeViewModel.DrawAtoms(AtomExtractor.FromApproximatePeptide(approximatePeptide));
            simulationRunner.TimestepCompleted += (sender, args) => Application.Current.Dispatcher.BeginInvoke(new Action(() => MoleculeViewModel.DrawAtoms(args.Atoms)));
            SimulationViewModel = new SimulationViewModel(simulationRunner);
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

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SimulationViewModel.StopSimulationCommand.Execute(null);
        }
    }
}
