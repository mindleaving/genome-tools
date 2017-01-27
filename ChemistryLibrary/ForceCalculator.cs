using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public class ForceCalculator
    {
        public ForceCalculatorResult CalculateForces(Molecule molecule)
        {
            var graph = molecule.MoleculeStructure;

            var forceLookup = new Dictionary<uint, Vector3D>();
            foreach (var edge in graph.Edges.Values)
            {
                var vertex1 = graph.Vertices[edge.Vertex1Id];
                var vertex2 = graph.Vertices[edge.Vertex2Id];
                var atom1 = (Atom) vertex1.Object;
                var atom2 = (Atom)vertex2.Object;
                var bond = (Bond) edge.Object;

                if (atom1.Position.Unit != atom2.Position.Unit)
                    throw new PhysicsException("Atoms do not have same position units");

                var v1v2Vector = atom1.Position.VectorTo(atom2.Position);
                var atomDistance = v1v2Vector.Magnitude().To(atom1.Position.Unit);
                var forceStrength = bond.BondEnergy.In(Unit.ElectronVolts)*(atomDistance - bond.BondLength).In(SIPrefix.Pico, Unit.Meter);

                if (!forceLookup.ContainsKey(vertex1.Id))
                    forceLookup.Add(vertex1.Id, forceStrength*v1v2Vector);
                else
                    forceLookup[vertex1.Id] += forceStrength*v1v2Vector;
                if (!forceLookup.ContainsKey(vertex2.Id))
                    forceLookup.Add(vertex2.Id, forceStrength * v1v2Vector);
                else
                    forceLookup[vertex2.Id] += -forceStrength * v1v2Vector;
            }

            // Valence electron repulsion
            foreach (var vertex in graph.Vertices.Values)
            {
                var currentAtom = (Atom) vertex.Object;
                var filledOuterOrbitals = currentAtom.OuterOrbitals.Where(o => o.IsFull).ToList();
                foreach (var orbital1 in filledOuterOrbitals)
                {
                    foreach (var orbital2 in filledOuterOrbitals)
                    {
                        if(ReferenceEquals(orbital1, orbital2))
                            continue;
                        // TODO: Apply repulsion
                    }
                }
            }
            return new ForceCalculatorResult(forceLookup);
        }
    }

    public class ForceCalculatorResult
    {
        public ForceCalculatorResult(Dictionary<uint, Vector3D> forceLookup)
        {
            ForceLookup = forceLookup;
        }

        public Dictionary<uint, Vector3D> ForceLookup { get; }
    }
}
