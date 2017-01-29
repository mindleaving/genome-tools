using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChemistryLibrary;
using Commons;

namespace MoleculeViewer
{
    public class SimulationViewModel : IDisposable
    {
        private readonly MoleculeViewModel moleculeViewModel;
        private Molecule Molecule => moleculeViewModel.Molecule;
        private readonly MoleculeDynamicsSimulator moleculeDynamicsSimulator;
        private CancellationTokenSource cancellationTokenSource;

        public SimulationViewModel(MoleculeViewModel moleculeViewModel)
        {
            this.moleculeViewModel = moleculeViewModel;
            moleculeDynamicsSimulator = new MoleculeDynamicsSimulator();
            moleculeDynamicsSimulator.OneIterationComplete += (sender, args) => moleculeViewModel.MoleculeHasBeenUpdated();
        }

        public UnitValue SimulationTime { get; set; } = 40.To(SIPrefix.Femto, Unit.Second);
        public UnitValue TimeStep { get; set; } = 4.To(SIPrefix.Femto, Unit.Second);

        public void RunSimulation()
        {
            //Task.Run(() =>
            //{
            //    for (int i = 0; i < 1000; i++)
            //    {
            //        var firstAtom = Molecule.Atoms.First();
            //        firstAtom.Position += new UnitPoint3D(SIPrefix.Pico, Unit.Meter, 0, 10, 10);
            //        moleculeViewModel.MoleculeHasBeenUpdated();
            //        Thread.Sleep(100);
            //    }
            //});
            var settings = new MoleculeDynamicsSimulationSettings
            {
                SimulationTime = SimulationTime,
                TimeStep = TimeStep
            };
            cancellationTokenSource = new CancellationTokenSource();
            moleculeDynamicsSimulator.MinimizeEnergy(Molecule, settings, cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
