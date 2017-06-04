using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ChemistryLibrary;
using ChemistryLibrary.Objects;
using ChemistryLibrary.Simulation;
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

        public UnitValue SimulationTime { get; set; } = 4.To(SIPrefix.Pico, Unit.Second);
        public UnitValue TimeStep { get; set; } = 8.To(SIPrefix.Femto, Unit.Second);

        public async Task RunSimulation()
        {
            var settings = new MoleculeDynamicsSimulationSettings
            {
                SimulationTime = SimulationTime,
                TimeStep = TimeStep
            };
            cancellationTokenSource = new CancellationTokenSource();
            await moleculeDynamicsSimulator.MinimizeEnergy(Molecule, customForces, settings, cancellationTokenSource.Token);
        }

        public void Dispose()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
