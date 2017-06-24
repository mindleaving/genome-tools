using System;
using System.Threading.Tasks;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public interface ISimulationRunner : IDisposable
    {
        Task SimulationTask { get; }
        bool IsSimulating { get; }
        UnitValue CurrentTime { get; }
        event EventHandler<SimulationTimestepCompleteEventArgs> TimestepCompleted;
        event EventHandler SimulationCompleted;
        void StartSimulation();
        void StopSimulation();
    }
}