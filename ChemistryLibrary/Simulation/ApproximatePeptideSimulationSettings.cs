﻿using Commons.Extensions;
using Commons.Physics;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class ApproximatePeptideSimulationSettings : MoleculeDynamicsSimulationSettings
    {
        public bool FreezeSecondaryStructures { get; set; }
        public UnitValue ReservoirTemperature { get; set; } = 37.To(Unit.Celsius);
        public bool UseCompactingForce { get; set; } = true;
        public bool UseRamachandranForce { get; set; } = true;
    }
}