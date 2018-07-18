using System.Windows.Input;
using ChemistryLibrary.Simulation;
using Commons.Extensions;
using Commons.Physics;

namespace MoleculeViewer.ViewModels
{
    public class SimulationViewModel : ViewModelBase
    {
        private readonly ISimulationRunner simulationRunner;

        public SimulationViewModel(ISimulationRunner simulationRunner)
        {
            this.simulationRunner = simulationRunner;
            StartSimulationCommand = new RelayCommand(simulationRunner.StartSimulation);
            StopSimulationCommand = new RelayCommand(simulationRunner.StopSimulation);
        }

        public UnitValue SimulationTime { get; set; } = 4.To(SIPrefix.Pico, Unit.Second);
        public UnitValue TimeStep { get; set; } = 8.To(SIPrefix.Femto, Unit.Second);

        public ICommand StartSimulationCommand { get; }
        public ICommand StopSimulationCommand { get; }
    }
}
