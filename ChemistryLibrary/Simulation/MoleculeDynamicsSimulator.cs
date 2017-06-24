using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public class MoleculeDynamicsSimulator : ISimulationRunner
    {
        private const double ForceLowerCutoff = 1e-50;

        private readonly List<CustomAtomForce> customAtomForces;
        private readonly MoleculeDynamicsSimulationSettings simulationSettings;
        private CancellationTokenSource cancellationTokenSource;
        private readonly object simulationControlLock = new object();

        public MoleculeDynamicsSimulator(Molecule molecule,
            List<CustomAtomForce> customAtomForces,
            MoleculeDynamicsSimulationSettings simulationSettings)
        {
            this.customAtomForces = customAtomForces;
            this.simulationSettings = simulationSettings;
            Molecule = molecule;
        }

        public Molecule Molecule { get; }
        public Task SimulationTask { get; private set; }
        public bool IsSimulating { get; private set; }
        public UnitValue CurrentTime { get; private set; }
        public event EventHandler<SimulationTimestepCompleteEventArgs> TimestepCompleted;
        public event EventHandler SimulationCompleted;

        public void StartSimulation()
        {
            if (IsSimulating)
                return;
            lock (simulationControlLock)
            {
                // Recheck
                if (IsSimulating)
                    return;
                cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                SimulationTask = Task.Run(() => RunSimulation(cancellationToken), cancellationToken);
                IsSimulating = true;
            }
        }

        public void StopSimulation()
        {
            if (!IsSimulating)
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

        private void RunSimulation(CancellationToken cancellationToken)
        {
            if (!Molecule.IsPositioned)
                Molecule.PositionAtoms();
            var currentAtomPositions = Molecule.MoleculeStructure.Vertices.Keys
                .ToDictionary(vId => vId, vId => Molecule.GetAtom(vId).Position);
            var lastNeighborhoodUpdate = 0.To(Unit.Second);
            var atomNeighborhoodMap = new AtomNeighborhoodMap(Molecule);
            for (var t = 0.To(Unit.Second); t < simulationSettings.SimulationTime; t += simulationSettings.TimeStep)
            {
                if(cancellationToken.IsCancellationRequested)
                    break;
                if (t - lastNeighborhoodUpdate > 400.To(SIPrefix.Femto, Unit.Second))
                    atomNeighborhoodMap.Update();

                var forces = ForceCalculator.CalculateForces(Molecule, atomNeighborhoodMap);
                AddCustomForces(Molecule, t, forces.ForceLookup, customAtomForces);
                ApplyAtomForces(Molecule, t, forces, simulationSettings);
                ApplyLonePairRepulsion(forces);
                // TODO: Redistribute electrons (either here or as a step after molecule is fully connected
                //WriteDebug(molecule);

                var newAtomPositions = Molecule.MoleculeStructure.Vertices.Keys
                    .ToDictionary(vId => vId, vId => Molecule.GetAtom(vId).Position);
                if (simulationSettings.StopSimulationWhenAtomAtRest && t > simulationSettings.ForceRampUpPeriod)
                {
                    var maximumPositionChange = currentAtomPositions.Keys
                        .Select(atom => currentAtomPositions[atom].DistanceTo(newAtomPositions[atom]).In(SIPrefix.Pico, Unit.Meter))
                        .Max();
                    if(maximumPositionChange/simulationSettings.TimeStep.Value < simulationSettings.MovementDetectionThreshold.Value)
                        break;
                }
                currentAtomPositions = newAtomPositions;
                OnTimestepCompleted(new SimulationTimestepCompleteEventArgs(t, null));
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
            MoleculeDynamicsSimulationSettings settings)
        {
            var maxVelocity = 1e2;
            var dT = settings.TimeStep.In(Unit.Second);
            foreach (var vertexForce in forces.ForceLookup)
            {
                var atom = molecule.GetAtom(vertexForce.Key);
                var force = vertexForce.Value;
                if (settings.ForceRampUp && elapsedTime < settings.ForceRampUpPeriod)
                {
                    var quotient = elapsedTime.Value/settings.ForceRampUpPeriod.Value;
                    force *= quotient;//Math.Pow(10, (int)(-100*quotient));
                }
                if (force.Magnitude() < ForceLowerCutoff)
                    continue;
                atom.Velocity += settings.TimeStep * (force.To(Unit.Newton) / atom.Mass);
                if (atom.Velocity.Magnitude().In(Unit.MetersPerSecond) > maxVelocity)
                    atom.Velocity *= maxVelocity / atom.Velocity.Magnitude().In(Unit.MetersPerSecond);
                atom.Position += dT*atom.Velocity;
                if (settings.ResetAtomVelocityAfterEachTimestep)
                    atom.Velocity = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);
                else
                    atom.Velocity *= 1.0; // TODO: Scale velocity to maintain a specific total energy, matching the environement's temperature
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

        private void OnTimestepCompleted(SimulationTimestepCompleteEventArgs e)
        {
            TimestepCompleted?.Invoke(this, e);
        }

        private void OnSimulationFinished()
        {
            SimulationCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
        }
    }
}