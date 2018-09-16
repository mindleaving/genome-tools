using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using ChemistryLibrary.Builders;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.IO;
using ChemistryLibrary.Objects;
using ChemistryLibrary.Simulation;
using ChemistryLibrary.Simulation.RamachadranPlotForce;
using Commons;
using Commons.Extensions;
using Commons.Physics;
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

            var filename = @"G:\Projects\HumanGenome\Protein-PDBs\2mgo.pdb";
            //var filename = @"G:\Projects\HumanGenome\AminoseqFiles\CFTR.aminoseq";
            ApproximatePeptide approximatePeptide;
            Peptide peptide;
            if (filename.ToLowerInvariant().EndsWith(".pdb"))
            {
                peptide = PeptideLoader.Load(filename);
                peptide.Molecule.MarkBackbone(peptide.MoleculeReference);
                peptide.Molecule.PositionAtoms(peptide.MoleculeReference.FirstAtomId, peptide.MoleculeReference.LastAtomId);
                approximatePeptide = ApproximatePeptideBuilder.FromPeptide(peptide);
            }
            else
            {
                approximatePeptide = ApproximatePeptideBuilder.FromSequence(new string('A', 8), 1);
                //approximatePeptide = ApproximatePeptideBuilder.FromSequence(File.ReadAllLines(filename).Aggregate((a,b) => a + b));
                approximatePeptide.UpdatePositions();
                peptide = new ApproximatePeptideCompleter(approximatePeptide).GetFullPeptide();
            }

            var simulationSettings = new ApproximatePeptideSimulationSettings
            {
                TimeStep = 2.To(SIPrefix.Femto, Unit.Second),
                SimulationTime = 10.To(SIPrefix.Nano, Unit.Second),
                ResetAtomVelocityAfterEachTimestep = false,
                UseCompactingForce = true,
                UseRamachandranForce = true
            };
            var ramachadranDataDirectory = @"G:\Projects\HumanGenome\ramachadranDistributions";
            var ramachandranPlotDistributionSource = new RamachandranPlotDistributionFileSource(ramachadranDataDirectory);
            //var ramachandranPlotDistributionSource = new RamachandranPlotDistributionFixedSource(
            //    new RamachandranPlotFixedDistribution(AminoAcidName.Alanine, new UnitPoint2D(-57.To(Unit.Degree), -47.To(Unit.Degree))));
            //var simulationRunner = ApproximatePeptideFoldingSimulatorFactory.Create(
            //    approximatePeptide, simulationSettings, ramachadranDataDirectory);
            var simulationRunner = new ApproximatePeptideFoldingSimulator(approximatePeptide,
                simulationSettings,
                new CompactingForceCalculator(), 
                new RamachandranForceCalculator(ramachandranPlotDistributionSource),
                new BondForceCalculator());
            //var simulationRunner = new MoleculeDynamicsSimulator(peptide.Molecule, new List<CustomAtomForce>(),
            //    new MoleculeDynamicsSimulationSettings
            //    {
            //        TimeStep = 2.To(SIPrefix.Femto, Unit.Second),
            //        SimulationTime = 10.To(SIPrefix.Pico, Unit.Second)
            //    });
            MoleculeViewModel = new MoleculeViewModel();
            //MoleculeViewModel.DrawAtoms(AtomExtractor.FromApproximatePeptide(approximatePeptide));
            MoleculeViewModel.DrawAtoms(AtomExtractor.FromMolecule(peptide.Molecule));
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
