using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;
using GenomeTools.ChemistryLibrary.Simulation.RamachadranPlotForce;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class ApproximatePeptideFoldingSimulator : ISimulationRunner
    {
        private readonly ApproximatePeptideSimulationSettings simulationSettings;
        private CancellationTokenSource cancellationTokenSource;
        private readonly object simulationControlLock = new object();
        private readonly CompactingForceCalculator compactnessForceCalculator;
        private readonly RamachandranForceCalculator ramachandranForceCalculator;
        private readonly BondForceCalculator bondForceCalculator;
        public Task SimulationTask { get; private set; }

        public ApproximatePeptide Peptide { get; }
        public bool IsSimulating { get; private set; }
        public UnitValue CurrentTime { get; private set; }
        public event EventHandler<SimulationTimestepCompleteEventArgs> TimestepCompleted;
        public event EventHandler SimulationCompleted;

        public ApproximatePeptideFoldingSimulator(ApproximatePeptide peptide, 
            ApproximatePeptideSimulationSettings simulationSettings,
            CompactingForceCalculator compactnessForceCalculator, 
            RamachandranForceCalculator ramachandranForceCalculator, 
            BondForceCalculator bondForceCalculator)
        {
            this.simulationSettings = simulationSettings;
            this.compactnessForceCalculator = compactnessForceCalculator;
            this.ramachandranForceCalculator = ramachandranForceCalculator;
            this.bondForceCalculator = bondForceCalculator;
            Peptide = peptide;

            if (peptide.AminoAcids.Count <= 3)
                simulationSettings.UseCompactingForce = false; // Cannot be done
        }

        public void StartSimulation()
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
                SimulationTask = Task.Run(() => RunSimulation(cancellationToken), cancellationToken);
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
                try
                {
                    SimulationTask?.Wait();
                }
                catch (AggregateException e)
                {
                }
                IsSimulating = false;
            }
        }

        private void RunSimulation(CancellationToken cancellationToken)
        {
            var simulationTime = simulationSettings.SimulationTime;
            var dT = simulationSettings.TimeStep;
            for (CurrentTime = 0.To(Unit.Second); CurrentTime < simulationTime; CurrentTime += dT)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var compactnessForces = simulationSettings.UseCompactingForce
                    ? compactnessForceCalculator.Calculate(CompactnessMeasurer.Measure(Peptide))
                    : new Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces>();
                var ramachandranForces = simulationSettings.UseRamachandranForce
                    ? ramachandranForceCalculator.Calculate(Peptide)
                    : new Dictionary<ApproximatedAminoAcid, ApproximateAminoAcidForces>();
                var bondForces = bondForceCalculator.Calculate(Peptide);
                foreach (var aminoAcid in Peptide.AminoAcids)
                {
                    var resultingForce = new ApproximateAminoAcidForces();
                    if (compactnessForces.ContainsKey(aminoAcid))
                    {
                        var compactnessForce = compactnessForces[aminoAcid];
                        resultingForce += compactnessForce;
                    }
                    if (ramachandranForces.ContainsKey(aminoAcid))
                    {
                        var ramachandranForce = ramachandranForces[aminoAcid];
                        resultingForce += ramachandranForce;
                    }
                    if (bondForces.ContainsKey(aminoAcid))
                    {
                        var bondForce = bondForces[aminoAcid];
                        resultingForce += bondForce;
                    }
                    ApplyForce(aminoAcid, resultingForce, dT, simulationSettings.ReservoirTemperature);
                }
                OnSimulationTimestepComplete(new SimulationTimestepCompleteEventArgs(CurrentTime, Peptide));
            }
            SimulationCompleted?.Invoke(this, EventArgs.Empty);
            IsSimulating = false;
        }

        private void ApplyForce(ApproximatedAminoAcid aminoAcid, 
            ApproximateAminoAcidForces resultingForces, 
            UnitValue timeStepSize,
            UnitValue reservoirTemperature)
        {
            var scaling = 1e0;
            var nitrogenForce = scaling*resultingForces.NitrogenForce;
            var nitrogenMass = PeriodicTable.GetSingleAtomMass(ElementName.Nitrogen);
            var nitrogenAcceleration = nitrogenForce / nitrogenMass;
            var nitrogenVelocityChange = nitrogenAcceleration * timeStepSize;
            aminoAcid.NitrogenVelocity += nitrogenVelocityChange;
            aminoAcid.NitrogenVelocity = InteractWithReservoir(aminoAcid.NitrogenVelocity, nitrogenMass, reservoirTemperature);
            aminoAcid.NitrogenPosition += aminoAcid.NitrogenVelocity * timeStepSize;
            if (simulationSettings.ResetAtomVelocityAfterEachTimestep)
                aminoAcid.NitrogenVelocity = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);

            var carbonAlphaForce = scaling * resultingForces.CarbonAlphaForce;
            var carbonAlphaMass = PeriodicTable.GetSingleAtomMass(ElementName.Carbon) + AminoAcidSideChainMassLookup.SideChainMasses[aminoAcid.Name];
            var carbonAlphaAcceleration = carbonAlphaForce / carbonAlphaMass;
            var carbonAlphaVelocityChange = carbonAlphaAcceleration * timeStepSize;
            aminoAcid.CarbonAlphaVelocity += carbonAlphaVelocityChange;
            aminoAcid.CarbonAlphaVelocity = InteractWithReservoir(aminoAcid.CarbonAlphaVelocity, carbonAlphaMass, reservoirTemperature);
            aminoAcid.CarbonAlphaPosition += aminoAcid.CarbonAlphaVelocity * timeStepSize;
            if (simulationSettings.ResetAtomVelocityAfterEachTimestep)
                aminoAcid.CarbonAlphaVelocity = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);

            var carbonForce = scaling * resultingForces.CarbonForce;
            var carbonMass = PeriodicTable.GetSingleAtomMass(ElementName.Carbon);
            var carbonAcceleration = carbonForce / carbonMass;
            var carbonVelocityChange = carbonAcceleration * timeStepSize;
            aminoAcid.CarbonVelocity += carbonVelocityChange;
            aminoAcid.CarbonVelocity = InteractWithReservoir(aminoAcid.CarbonVelocity, carbonMass, reservoirTemperature);
            aminoAcid.CarbonPosition += aminoAcid.CarbonVelocity * timeStepSize;
            if (simulationSettings.ResetAtomVelocityAfterEachTimestep)
                aminoAcid.CarbonVelocity = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);
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
                const double DampingSpeed = 0.5; // Between 0 (no damping) and 1 (full damping)
                var damping = 1 - DampingSpeed * (velocityMagnitude - targetVelocity).Value / velocityMagnitude.Value;
                return damping * velocity;
            }
            return velocity;
        }

        private void OnSimulationTimestepComplete(SimulationTimestepCompleteEventArgs e)
        {
            TimestepCompleted?.Invoke(this, e);
        }

        public void Dispose()
        {
            StopSimulation();
            cancellationTokenSource?.Dispose();
            SimulationTask?.Dispose();
        }
    }
}
