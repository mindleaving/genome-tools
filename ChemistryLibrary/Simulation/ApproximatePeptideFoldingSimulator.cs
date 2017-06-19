using System;
using System.Threading;
using System.Threading.Tasks;
using ChemistryLibrary.DataLookups;
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
                cancellationToken.ThrowIfCancellationRequested();

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
                    ApplyForce(aminoAcid, resultingForce, dT, simulationSettings.ReservoirTemperature);
                }
            }
        }

        private void ApplyForce(ApproximatedAminoAcid aminoAcid, 
            ApproximateAminoAcidForces resultingForces, 
            UnitValue timeStepSize,
            UnitValue reservoirTemperature)
        {
            var nitrogenForce = resultingForces.NitrogenForce;
            var nitrogenMass = PeriodicTable.GetMass(ElementName.Nitrogen);
            var nitrogenAcceleration = nitrogenForce / nitrogenMass;
            var nitrogenVelocityChange = nitrogenAcceleration * timeStepSize;
            aminoAcid.NitrogenVelocity += nitrogenVelocityChange;
            aminoAcid.NitrogenVelocity = InteractWithReservoir(aminoAcid.NitrogenVelocity, nitrogenMass, reservoirTemperature);
            aminoAcid.NitrogenPosition += aminoAcid.NitrogenVelocity * timeStepSize;

            var carbonAlphaForce = resultingForces.CarbonAlphaForce;
            var carbonAlphaMass = PeriodicTable.GetMass(ElementName.Carbon) + AminoAcidSideChainMassLookup.SideChainMasses[aminoAcid.Name];
            var carbonAlphaAcceleration = carbonAlphaForce / carbonAlphaMass;
            var carbonAlphaVelocityChange = carbonAlphaAcceleration * timeStepSize;
            aminoAcid.CarbonAlphaVelocity += carbonAlphaVelocityChange;
            aminoAcid.CarbonAlphaVelocity = InteractWithReservoir(aminoAcid.CarbonAlphaVelocity, carbonAlphaMass, reservoirTemperature);
            aminoAcid.CarbonAlphaPosition += aminoAcid.CarbonAlphaVelocity * timeStepSize;

            var carbonForce = resultingForces.CarbonForce;
            var carbonMass = PeriodicTable.GetMass(ElementName.Carbon);
            var carbonAcceleration = carbonForce / carbonMass;
            var carbonVelocityChange = carbonAcceleration * timeStepSize;
            aminoAcid.CarbonVelocity += carbonVelocityChange;
            aminoAcid.CarbonVelocity = InteractWithReservoir(aminoAcid.CarbonVelocity, carbonMass, reservoirTemperature);
            aminoAcid.CarbonPosition += aminoAcid.CarbonVelocity * timeStepSize;
        }

        private UnitVector3D InteractWithReservoir(
            UnitVector3D velocity,
            UnitValue mass,
            UnitValue reservoirTemperature)
        {
            var velocityMagnitude = velocity.Magnitude();
            var targetVelocity = Math.Sqrt(3*(PhysicalConstants.BoltzmannsConstant*reservoirTemperature/mass).Value).To(Unit.MetersPerSecond);
            if(velocityMagnitude > targetVelocity)
            {
                const double dampingSpeed = 0.5; // Between 0 (no damping) and 1 (full damping)
                var damping = 1 - dampingSpeed * (velocityMagnitude - targetVelocity).Value / velocityMagnitude.Value;
                return damping * velocity;
            }
            return velocity;
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
        public UnitValue ReservoirTemperature { get; set; } = 37.To(Unit.Celcius);
    }
}
