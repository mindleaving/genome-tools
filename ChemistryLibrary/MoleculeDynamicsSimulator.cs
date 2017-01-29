using System;
using System.Threading;
using System.Threading.Tasks;
using Commons;

namespace ChemistryLibrary
{
    public class MoleculeDynamicsSimulator
    {
        public Task MinimizeEnergy(Molecule molecule, MoleculeDynamicsSimulationSettings settings,
            CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => Simulate(molecule, settings, cancellationToken, true), cancellationToken);
        }

        public Task RunSimulation(Molecule molecule, MoleculeDynamicsSimulationSettings settings,
            CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => Simulate(molecule, settings, cancellationToken), cancellationToken);
        }

        private void Simulate(Molecule molecule, MoleculeDynamicsSimulationSettings settings,
            CancellationToken cancellationToken, bool zeroAtomMomentum = false)
        {
            var forceCalculator = new ForceCalculator();
            for (var t = 0.To(Unit.Second); t < settings.SimulationTime; t += settings.TimeStep)
            {
                if(cancellationToken.IsCancellationRequested)
                    break;
                var forces = forceCalculator.CalculateForces(molecule);
                ApplyBondForces(molecule, settings, zeroAtomMomentum, forces);
                foreach (var lonePairForce in forces.LonePairForceLookup)
                {
                    var orbital = lonePairForce.Key;
                    var atom = orbital.Atom;
                    var force = 
                }

                OnOneIterationComplete();
            }
            OnSimulationFinished();
        }

        private static void ApplyBondForces(Molecule molecule, MoleculeDynamicsSimulationSettings settings,
            bool zeroAtomMomentum, ForceCalculatorResult forces)
        {
            foreach (var vertexForce in forces.ForceLookup)
            {
                var vertex = molecule.MoleculeStructure.Vertices[vertexForce.Key];
                var force = vertexForce.Value;
                var atom = (Atom) vertex.Object;
                atom.Velocity += (force/atom.Mass)*settings.TimeStep;
                atom.Position += atom.Velocity*settings.TimeStep;
                if (zeroAtomMomentum)
                    atom.Velocity = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);
            }
        }

        public event EventHandler OneIterationComplete;
        private void OnOneIterationComplete()
        {
            OneIterationComplete?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SimulationFinished;
        private void OnSimulationFinished()
        {
            SimulationFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    public class MoleculeDynamicsSimulationSettings
    {
        public UnitValue TimeStep { get; set; }
        public UnitValue SimulationTime { get; set; }
    }
}
