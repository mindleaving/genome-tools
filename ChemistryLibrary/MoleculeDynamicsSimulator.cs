using System;
using System.Collections.Generic;
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

        public Task MinimizeEnergy(Molecule molecule,
            List<CustomAtomForce> customForces,
            MoleculeDynamicsSimulationSettings settings, 
            CancellationToken cancellationToken)
        {
            settings.StopSimulationWhenAtomAtRest = false;
            settings.RampUp = true;
            settings.RampUpPeriod = 500*settings.TimeStep;
            return Task.Factory.StartNew(() => Simulate(molecule, customForces, settings, cancellationToken, true), cancellationToken);
        }

        public Task RunSimulation(Molecule molecule, 
            List<CustomAtomForce> customForces,
            MoleculeDynamicsSimulationSettings settings,
            CancellationToken cancellationToken)
        {
            settings.StopSimulationWhenAtomAtRest = false;
            settings.RampUp = true;
            settings.RampUpPeriod = 500 * settings.TimeStep;
            return Task.Factory.StartNew(() => Simulate(molecule, customForces, settings, cancellationToken), cancellationToken);
        }

        private void Simulate(Molecule molecule,
            List<CustomAtomForce> customForces,
            MoleculeDynamicsSimulationSettings settings,
            CancellationToken cancellationToken, 
            bool zeroAtomMomentum = false)
        {
            if (!molecule.IsPositioned)
                molecule.PositionAtoms();
            var currentAtomPositions = molecule.MoleculeStructure.Vertices.Keys
                .ToDictionary(vId => vId, vId => molecule.GetAtom(vId).Position);
            var lastNeighborhoodUpdate = 0.To(Unit.Second);
            var atomNeighborhoodMap = new AtomNeighborhoodMap(molecule);
            for (var t = 0.To(Unit.Second); t < settings.SimulationTime; t += settings.TimeStep)
            {
                if(cancellationToken.IsCancellationRequested)
                    break;
                if(t - lastNeighborhoodUpdate > 400.To(SIPrefix.Femto, Unit.Second))
                    atomNeighborhoodMap.Update();

                var forces = ForceCalculator.CalculateForces(molecule, atomNeighborhoodMap);
                AddCustomForces(molecule, t, forces.ForceLookup, customForces);
                ApplyAtomForces(molecule, t, forces, settings, zeroAtomMomentum);
                ApplyLonePairRepulsion(forces);
                //WriteDebug(molecule);

                var newAtomPositions = molecule.MoleculeStructure.Vertices.Keys
                    .ToDictionary(vId => vId, vId => molecule.GetAtom(vId).Position);
                if (settings.StopSimulationWhenAtomAtRest && t > settings.RampUpPeriod)
                {
                    var maximumPositionChange = currentAtomPositions.Keys
                        .Select(atom => currentAtomPositions[atom].DistanceTo(newAtomPositions[atom]))
                        .Max();
                    if(maximumPositionChange/settings.TimeStep.Value < settings.MovementDetectionThreshold.Value)
                        break;
                }
                currentAtomPositions = newAtomPositions;
                OnOneIterationComplete();
            }
            OnSimulationFinished();
        }

        private void AddCustomForces(Molecule molecule,
            UnitValue elapsedTime,
            Dictionary<uint, Vector3D> forceLookup, 
            List<CustomAtomForce> customForces)
        {
            foreach (var customForce in customForces)
            {
                var atom = molecule.GetAtom(customForce.AtomVertex);
                var force = customForce.ForceFunc(atom, elapsedTime);
                forceLookup[customForce.AtomVertex] += force;
            }
        }

        private void WriteDebug(Molecule molecule)
        {
            var oxygen = molecule.Atoms.Single(atom => atom.Element == ElementName.Oxygen);
            var fullOuterOrbitals = oxygen.OuterOrbitals.Where(o => o.IsFull);
            var output = fullOuterOrbitals
                .Select(o => o.Atom.Position.VectorTo(o.MaximumElectronDensityPosition).Normalize())
                .Select(v => v.X + ";" + v.Y + ";" + v.Z)
                .Aggregate((a,b) => a + ";" + b)
                + Environment.NewLine;
            File.AppendAllText(@"G:\Projects\HumanGenome\SpherePointDistribution_debug.csv", output);
        }

        private static void ApplyAtomForces(Molecule molecule, 
            UnitValue elapsedTime, 
            ForceCalculatorResult forces, 
            MoleculeDynamicsSimulationSettings settings, 
            bool zeroAtomMomentum)
        {
            var maxVelocity = 1e2;
            var dT = settings.TimeStep.In(Unit.Second);
            foreach (var vertexForce in forces.ForceLookup)
            {
                var atom = molecule.GetAtom(vertexForce.Key);
                var force = vertexForce.Value;
                if (settings.RampUp && elapsedTime < settings.RampUpPeriod)
                {
                    var quotient = elapsedTime.Value/settings.RampUpPeriod.Value;
                    force *= quotient;//Math.Pow(10, (int)(-100*quotient));
                }
                if (force.Magnitude() < ForceLowerCutoff)
                    continue;
                atom.Velocity += settings.TimeStep.In(Unit.Second)/ atom.Mass.In(Unit.Kilogram) * force;
                if (atom.Velocity.Magnitude() > maxVelocity)
                    atom.Velocity *= maxVelocity / atom.Velocity.Magnitude();
                atom.Position += dT*atom.Velocity;
                if (zeroAtomMomentum)
                    atom.Velocity = new Vector3D(0, 0, 0);
                else
                    atom.Velocity *= 0.5;
            }
        }

        private static void ApplyLonePairRepulsion(ForceCalculatorResult forces)
        {
            foreach (var lonePairForce in forces.LonePairForceLookup)
            {
                var orbital = lonePairForce.Key;
                var atom = orbital.Atom;
                var force = lonePairForce.Value;
                if(force.Magnitude() < ForceLowerCutoff)
                    continue;
                var displacementDirection = force.Normalize();
                var atomRadius = atom.Radius.Value;

                var lonePairVector = atom.Position.VectorTo(orbital.MaximumElectronDensityPosition);
                var displacementNormal = displacementDirection.ProjectOnto(lonePairVector.Normalize());
                var tangentialDisplacement = displacementDirection - displacementNormal;
                var displacedLonePair = orbital.MaximumElectronDensityPosition
                                        + atomRadius*tangentialDisplacement;
                lonePairVector = atom.Position.VectorTo(displacedLonePair);
                var scaling = 0.85*atomRadius / lonePairVector.Magnitude();
                orbital.MaximumElectronDensityPosition = atom.Position + scaling*lonePairVector;
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
        public UnitValue MovementDetectionThreshold { get; set; } = 150.To(SIPrefix.Milli, Unit.MetersPerSecond);
        public bool RampUp { get; set; }
        public UnitValue RampUpPeriod { get; set; }
    }
}
