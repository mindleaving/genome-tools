using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public class ForceCalculator
    {
        public static ForceCalculatorResult CalculateForces(Molecule molecule)
        {
            var graph = molecule.MoleculeStructure;

            var forceLookup = CalculateBondForces(graph);
            var lonePairForceLookup = CalculateLonePairRepulsion(graph, forceLookup);
            if(forceLookup.Values.Any(v => v.X.Value.IsNaN()))
                throw new Exception("Force cannot be 'NaN'");
            if (lonePairForceLookup.Values.Any(v => v.X.Value.IsNaN()))
                throw new Exception("Lone pair repulsion cannot be 'NaN'");
            return new ForceCalculatorResult(forceLookup, lonePairForceLookup);
        }

        private static Dictionary<uint, UnitVector3D> CalculateBondForces(Graph graph)
        {
            var forceLookup = graph.Vertices.Keys.ToDictionary(x => x, x => new UnitVector3D(Unit.Newton, 0, 0, 0));

            // Bond force
            foreach (var edge in graph.Edges.Values)
            {
                var vertex1 = graph.Vertices[edge.Vertex1Id];
                var vertex2 = graph.Vertices[edge.Vertex2Id];
                var atom1 = (Atom) vertex1.Object;
                var atom2 = (Atom) vertex2.Object;
                var bond = (Bond) edge.Object;

                var v1v2Vector = atom1.Position.VectorTo(atom2.Position);
                var forceDirection = v1v2Vector.In(Unit.Meter).Normalize();
                var atomDistance = v1v2Vector.Magnitude();
                var forceStrength = -1e1*bond.BondEnergy.In(Unit.ElectronVolts)
                    *(atomDistance - bond.BondLength).In(Unit.Meter)
                    .To(Unit.Newton);

                forceLookup[vertex1.Id] += forceStrength*forceDirection;
                forceLookup[vertex2.Id] += -forceStrength*forceDirection;
            }
            return forceLookup;
        }

        private static Dictionary<Orbital, UnitVector3D> CalculateLonePairRepulsion(Graph graph, Dictionary<uint, UnitVector3D> forceLookup)
        {
            var lonePairForceLookup = new Dictionary<Orbital, UnitVector3D>();
            foreach (var vertex in graph.Vertices.Values)
            {
                var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(graph, vertex).ToList();
                var currentAtom = (Atom) vertex.Object;
                var filledOuterOrbitals = currentAtom.OuterOrbitals.Where(o => o.IsFull).ToList();
                var orbitalNeighborVertexMap = MapBondOrbitalToNeighborVertex(
                    filledOuterOrbitals, currentAtom, adjacentVertices);
                foreach (var orbital1 in filledOuterOrbitals)
                {
                    foreach (var orbital2 in filledOuterOrbitals)
                    {
                        if (ReferenceEquals(orbital1, orbital2))
                            continue;
                        var repulsiveForce = CalculateRepulsiveForce(orbital1, orbital2);

                        if (orbital1.IsPartOfBond)
                        {
                            var neighborVertex = orbitalNeighborVertexMap[orbital1];
                            forceLookup[neighborVertex.Id] += repulsiveForce;
                        }
                        else
                        {
                            if(!lonePairForceLookup.ContainsKey(orbital1))
                                lonePairForceLookup.Add(orbital1, new UnitVector3D(Unit.Newton, 0,0,0));
                            lonePairForceLookup[orbital1] += repulsiveForce;
                        }
                        if (orbital2.IsPartOfBond)
                        {
                            var neighborVertex = orbitalNeighborVertexMap[orbital2];
                            forceLookup[neighborVertex.Id] += -repulsiveForce;
                        }
                        else
                        {
                            if (!lonePairForceLookup.ContainsKey(orbital2))
                                lonePairForceLookup.Add(orbital2, new UnitVector3D(Unit.Newton, 0, 0, 0));
                            lonePairForceLookup[orbital2] += -repulsiveForce;
                        }
                    }
                }
            }
            return lonePairForceLookup;
        }

        private static Dictionary<Orbital, Vertex> MapBondOrbitalToNeighborVertex(List<Orbital> filledOuterOrbitals, Atom currentAtom,
            List<Vertex> adjacentVertices)
        {
            var orbitalNeighborVertexMap = filledOuterOrbitals
                .Where(o => o.IsPartOfBond)
                .ToDictionary(
                    o => o,
                    o => {
                        var bond = o.AssociatedBond;
                        var neighborAtom = bond.Atom1.Equals(currentAtom) ? bond.Atom2 : bond.Atom1;
                        return adjacentVertices.Single(v => v.Object.Equals(neighborAtom));
                    });
            return orbitalNeighborVertexMap;
        }

        private static UnitVector3D CalculateRepulsiveForce(Orbital orbital1, Orbital orbital2)
        {
            var distance = orbital1.MaximumElectronDensityPosition
                .DistanceTo(orbital2.MaximumElectronDensityPosition);
            if(distance.Value == 0)
                return new UnitVector3D(Unit.Newton, 0,0,0);
            var forceVector = orbital1.MaximumElectronDensityPosition.In(Unit.Meter)
                .VectorTo(orbital2.MaximumElectronDensityPosition.In(Unit.Meter))
                .Normalize();
            var chargeProduct = PhysicalConstants.CoulombsConstant
                *PhysicalConstants.ElementaryCharge
                *PhysicalConstants.ElementaryCharge;
            var repulsiveForce = -(chargeProduct / (distance*distance))*forceVector;
            return 1e-3*repulsiveForce;
        }
    }

    public class ForceCalculatorResult
    {
        public ForceCalculatorResult(Dictionary<uint, UnitVector3D> forceLookup, 
            Dictionary<Orbital, UnitVector3D> lonePairForceLookup)
        {
            ForceLookup = forceLookup;
            LonePairForceLookup = lonePairForceLookup;
        }

        public Dictionary<uint, UnitVector3D> ForceLookup { get; }
        public Dictionary<Orbital, UnitVector3D> LonePairForceLookup { get; }
    }
}
