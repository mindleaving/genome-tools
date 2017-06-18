using System;
using System.Threading;
using System.Threading.Tasks;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public class ApproximatePeptideFoldingSimulator : IDisposable
    {
        private CancellationTokenSource cancellationTokenSource;
        private readonly object simulationControlLock = new object();
        public Task SimulationTask { get; private set; }

        public ApproximatePeptide Peptide { get; }
        public bool IsSimulating { get; private set; }
        public UnitValue CurrentTime { get; private set; }

        public ApproximatePeptideFoldingSimulator(ApproximatePeptide peptide)
        {
            Peptide = peptide;
        }

        public void StartSimulation(ApproximatePeptideSimulationSettings simulationSettings)
        {
            if (IsSimulating)
                return;
            lock (simulationControlLock)
            {
                // Recheck
                if(IsSimulating)
                    return;
                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                SimulationTask = Task.Run(() => RunSimulation(simulationSettings, cancellationToken), cancellationToken);
                IsSimulating = true;
            }
        }

        public void StopSimulation()
        {
            if(!IsSimulating)
                return;
            lock (simulationControlLock)
            {
                if (!IsSimulating)
                    return;
                cancellationTokenSource?.Cancel();
                SimulationTask?.Wait();
                IsSimulating = false;
            }
        }

        private void RunSimulation(ApproximatePeptideSimulationSettings simulationSettings,
            CancellationToken cancellationToken)
        {
            var ramachadranDataDirectory = @"G:\Projects\HumanGenome\ramachadranDistributions";
            var compactnessForceCalculator = new CompactingForceCalculator();
            var ramachadranForceCalculator = new RamachadranForceCalculator(ramachadranDataDirectory);

            var simulationTime = simulationSettings.SimulationTime;
            var dT = simulationSettings.TimeStep;
            for (CurrentTime = 0.To(Unit.Second); CurrentTime < simulationTime; CurrentTime += dT)
            {
                var compactnessMeasurerResult = CompactnessMeasurer.Measure(Peptide);
                var compactnessForces = compactnessForceCalculator.Calculate(compactnessMeasurerResult);
                var ramachandranForces = ramachadranForceCalculator.CalculateForce(Peptide);
                foreach (var aminoAcid in Peptide.AminoAcids)
                {
                    var resultingForce = new ApproximateAminoAcidForces();
                    if(compactnessForces.ContainsKey(aminoAcid))
                    {
                        var compactnessForce = compactnessForces[aminoAcid];
                        resultingForce += compactnessForce;
                    }
                    if (ramachandranForces.ContainsKey(aminoAcid))
                    {
                        var ramachandranForce = ramachandranForces[aminoAcid];
                        resultingForce += ramachandranForce;
                    }

                }
            }
        }

        public void Dispose()
        {
            StopSimulation();
            cancellationTokenSource?.Dispose();
            SimulationTask?.Dispose();
        }
    }

    public class ApproximatePeptideSimulationSettings : MoleculeDynamicsSimulationSettings
    {
        public bool FreezeSecondaryStructures { get; set; }
    }
}
