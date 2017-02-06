using System;
using System.Collections.Generic;
using System.Threading;
using ChemistryLibrary;
using Commons;

namespace MoleculeViewer
{
    public class SimulationViewModel : ViewModelBase, IDisposable
    {
        private readonly MoleculeViewModel moleculeViewModel;
        private readonly List<CustomAtomForce> customForces;
        private Molecule Molecule => moleculeViewModel.Molecule;
        private readonly MoleculeDynamicsSimulator moleculeDynamicsSimulator;
        private CancellationTokenSource cancellationTokenSource;

        public SimulationViewModel(MoleculeViewModel moleculeViewModel, List<CustomAtomForce> customForces)
        {
            this.moleculeViewModel = moleculeViewModel;
            this.customForces = customForces;
            moleculeDynamicsSimulator = new MoleculeDynamicsSimulator();
            moleculeDynamicsSimulator.OneIterationComplete += (sender, args) => moleculeViewModel.MoleculeHasBeenUpdated();
        }

        public UnitValue SimulationTime { get; set; } = 10.To(SIPrefix.Nano, Unit.Second);
        public UnitValue TimeStep { get; set; } = 2.To(SIPrefix.Femto, Unit.Second);

        public void RunSimulation()
        {
            var settings = new MoleculeDynamicsSimulationSettings
            {
                SimulationTime = SimulationTime,
                TimeStep = TimeStep
            };
            cancellationTokenSource = new CancellationTokenSource();
            moleculeDynamicsSimulator.MinimizeEnergy(Molecule, customForces, settings, cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
