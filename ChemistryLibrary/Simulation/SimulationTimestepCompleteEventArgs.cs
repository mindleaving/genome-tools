using System;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public class SimulationTimestepCompleteEventArgs : EventArgs
    {
        public SimulationTimestepCompleteEventArgs(UnitValue t, ApproximatePeptide peptide)
        {
            SimulationTime = t;
            PeptideCopy = peptide.DeepClone();
        }

        public UnitValue SimulationTime { get; }
        public ApproximatePeptide PeptideCopy { get; }
    }
}