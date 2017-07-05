﻿using ChemistryLibrary.Objects;
using ChemistryLibrary.Simulation.RamachadranPlotForce;

namespace ChemistryLibrary.Simulation
{
    public static class ApproximatePeptideFoldingSimulatorFactory
    {
        public static ApproximatePeptideFoldingSimulator Create(ApproximatePeptide peptide,
            ApproximatePeptideSimulationSettings simulationSettings,
            string ramachadranDataDirectory)
        {
            var compactnessForceCalculator = new CompactingForceCalculator();

            var ramachandranPlotDistributionSource = new RamachandranPlotDistributionFileSource(ramachadranDataDirectory);
            var ramachadranForceCalculator = new RamachandranForceCalculator(ramachandranPlotDistributionSource);
            var bondForceCalculator = new BondForceCalculator();

            return new ApproximatePeptideFoldingSimulator(peptide, 
                simulationSettings,
                compactnessForceCalculator,
                ramachadranForceCalculator,
                bondForceCalculator);
        }
    }
}
