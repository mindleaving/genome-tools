using Commons.Extensions;
using Commons.Physics;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class MoleculeDynamicsSimulationSettings
    {
        public UnitValue TimeStep { get; set; }
        public UnitValue SimulationTime { get; set; }
        public bool StopSimulationWhenAtomAtRest { get; set; }
        public UnitValue MovementDetectionThreshold { get; set; } = 150.To(SIPrefix.Milli, Unit.MetersPerSecond);
        public bool ForceRampUp { get; set; }
        public UnitValue ForceRampUpPeriod { get; set; }
        public bool ResetAtomVelocityAfterEachTimestep { get; set; }
    }
}
