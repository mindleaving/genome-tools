using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons;

namespace ChemistryLibrary
{
    public class MoleculeDynamicsSimulator
    {
        private const double ForceLowerCutoff = 1e-50;

        public Task MinimizeEnergy(Molecule molecule, MoleculeDynamicsSimulationSettings settings,
            CancellationToken cancellationToken)
        {
            settings.StopSimulationWhenAtomAtRest = false;
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
            var currentAtomPositions = molecule.Atoms.ToDictionary(atom => atom, atom => atom.Position.In(SIPrefix.Pico, Unit.Meter));
            for (var t = 0.To(Unit.Second); t < settings.SimulationTime; t += settings.TimeStep)
            {
                if(cancellationToken.IsCancellationRequested)
                    break;
                var forces = ForceCalculator.CalculateForces(molecule);
                ApplyBondForces(molecule, settings, zeroAtomMomentum, forces);
                ApplyLonePairRepulsion(forces);
                WriteDebug(molecule);

                var newAtomPositions = molecule.Atoms.ToDictionary(atom => atom, atom => atom.Position.In(SIPrefix.Pico, Unit.Meter));
                if (settings.StopSimulationWhenAtomAtRest)
                {
                    var maximumPositionChange = currentAtomPositions.Keys
                        .Select(atom => currentAtomPositions[atom].DistanceTo(newAtomPositions[atom]))
                        .Max()
                        .To(SIPrefix.Pico, Unit.Meter);
                    if(maximumPositionChange < settings.MovementDetectionThreshold)
                        break;
                }
                currentAtomPositions = newAtomPositions;
                OnOneIterationComplete();
            }
            OnSimulationFinished();
        }

        private void WriteDebug(Molecule molecule)
        {
            var oxygen = molecule.Atoms.Single(atom => atom.Element == ElementName.Oxygen);
            var fullOuterOrbitals = oxygen.OuterOrbitals.Where(o => o.IsFull);
            var output = fullOuterOrbitals
                .Select(o => o.Atom.Position.VectorTo(o.MaximumElectronDensityPosition).In(SIPrefix.Pico, Unit.Meter).Normalize())
                .Select(v => v.X + ";" + v.Y + ";" + v.Z)
                .Aggregate((a,b) => a + ";" + b)
                + Environment.NewLine;
            File.AppendAllText(@"G:\Projects\HumanGenome\SpherePointDistribution_debug.csv", output);
        }

        private static void ApplyLonePairRepulsion(ForceCalculatorResult forces)
        {
            foreach (var lonePairForce in forces.LonePairForceLookup)
            {
                var orbital = lonePairForce.Key;
                var atom = orbital.Atom;
                var force = lonePairForce.Value;
                if(force.Magnitude().Value < ForceLowerCutoff)
                    continue;
                var displacementDirection = force.In(Unit.Newton).Normalize();
                var atomRadius = atom.Radius;

                var lonePairVector = atom.Position.VectorTo(orbital.MaximumElectronDensityPosition);
                var displacementNormal = displacementDirection.ProjectOnto(lonePairVector.In(SIPrefix.Pico, Unit.Meter));
                var tangentialDisplacement = displacementDirection - displacementNormal;
                var displacedLonePair = orbital.MaximumElectronDensityPosition
                                        + 1e-2*atomRadius*tangentialDisplacement;
                lonePairVector = atom.Position.VectorTo(displacedLonePair);
                var scaling = atomRadius.In(SIPrefix.Pico, Unit.Meter) /
                    lonePairVector.Magnitude().In(SIPrefix.Pico, Unit.Meter);
                orbital.MaximumElectronDensityPosition = atom.Position + scaling*lonePairVector;
            }
        }

        private static void ApplyBondForces(Molecule molecule, MoleculeDynamicsSimulationSettings settings,
            bool zeroAtomMomentum, ForceCalculatorResult forces)
        {
            foreach (var vertexForce in forces.ForceLookup)
            {
                var vertex = molecule.MoleculeStructure.Vertices[vertexForce.Key];
                var force = vertexForce.Value;
                if (force.Magnitude().Value < ForceLowerCutoff)
                    continue;
                var atom = (Atom) vertex.Object;
                atom.Velocity += force/atom.Mass*settings.TimeStep;
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
        public bool StopSimulationWhenAtomAtRest { get; set; }
        public UnitValue MovementDetectionThreshold { get; set; } = 1.To(SIPrefix.Pico, Unit.Meter);
    }
}
