using Commons;

namespace ChemistryLibrary.Simulation
{
    public class ApproximatePeptideSimulationSettings : MoleculeDynamicsSimulationSettings
    {
        public bool FreezeSecondaryStructures { get; set; }
        public UnitValue ReservoirTemperature { get; set; } = 37.To(Unit.Celcius);
    }
}