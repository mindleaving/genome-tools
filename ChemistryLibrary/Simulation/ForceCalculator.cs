using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public static class ForceCalculator
    {
        public static ForceCalculatorResult CalculateForces(Molecule molecule, AtomNeighborhoodMap neighborhoodMap = null)
        {
            if (!molecule.IsPositioned)
                molecule.PositionAtoms();
            if (neighborhoodMap == null)
                neighborhoodMap = new AtomNeighborhoodMap(molecule);
            var graph = molecule.MoleculeStructure;

            var forceLookup = CalculateBondForces(graph);
            //CalculateIonicForces(molecule, forceLookup, neighborhoodMap);
            CalculateAtomShellRepulsion(molecule, forceLookup, neighborhoodMap);
            var lonePairForceLookup = CalculateLonePairRepulsion(graph, forceLookup);
            if(forceLookup.Values.Any(v => v.X.IsNaN()))
                throw new Exception("Force cannot be 'NaN'");
            if (lonePairForceLookup.Values.Any(v => v.X.IsNaN()))
                throw new Exception("Lone pair repulsion cannot be 'NaN'");
            return new ForceCalculatorResult(forceLookup, lonePairForceLookup, neighborhoodMap);
        }

        private static Dictionary<uint, Vector3D> CalculateBondForces(Graph<Atom,SimpleBond> graph)
        {
            var forceLookup = graph.Vertices.ToDictionary(
                kvp => kvp.Id, 
                kvp => new Vector3D(0, 0, 0));

            // Bond force
            foreach (var edge in graph.Edges)
            {
                var vertex1 = graph.GetVertexFromId(edge.Vertex1Id);
                var vertex2 = graph.GetVertexFromId(edge.Vertex2Id);
                var atom1 = vertex1.Object;
                var atom2 = vertex2.Object;
                var bond = (OrbitalBond)edge.Object;

                var v1V2Vector = atom1.Position.VectorTo(atom2.Position);
                var forceDirection = v1V2Vector.Normalize().ToVector3D();
                var atomDistance = v1V2Vector.Magnitude();
                var forceStrength = 1e4*-bond.BondEnergy.In(Unit.ElectronVolts)
                                    *(atomDistance - 0.9*bond.BondLength);

                forceLookup[vertex1.Id] += forceStrength*forceDirection;
                forceLookup[vertex2.Id] += -forceStrength*forceDirection;
            }
            return forceLookup;
        }

        private static void CalculateIonicForces(Molecule molecule,
            Dictionary<uint, Vector3D> forceLookup,
            AtomNeighborhoodMap neighborhoodMap)
        {
            var vertexIds = molecule.MoleculeStructure.Vertices.Select(v => v.Id);
            foreach (var vertexId in vertexIds)
            {
                var atom = molecule.GetAtom(vertexId);
                var charge1 = atom.EffectiveCharge;
                var atom1Position = atom.Position;

                var neighborhood = neighborhoodMap.GetNeighborhood(vertexId);
                foreach (var neighborVertexId in neighborhood)
                {
                    if (neighborVertexId <= vertexId)
                        continue;
                    var neighborAtom = molecule.GetAtom(neighborVertexId);
                    var charge2 = neighborAtom.EffectiveCharge;
                    var r = atom1Position.VectorTo(neighborAtom.Position);
                    var distance = r.Magnitude().In(SIPrefix.Pico, Unit.Meter);
                    var chargeProduct = PhysicalConstants.CoulombsConstant.Value
                                        *charge1.Value
                                        *charge2.Value;
                    var ionicForce = -5*1e-1*(chargeProduct/(distance*distance))*r.Normalize().ToVector3D();

                    forceLookup[vertexId] += ionicForce;
                    forceLookup[neighborVertexId] += -ionicForce;
                }
            }
        }

        private static void CalculateAtomShellRepulsion(Molecule molecule,
            Dictionary<uint, Vector3D> forceLookup,
            AtomNeighborhoodMap neighborhoodMap)
        {
            var vertices = molecule.MoleculeStructure.Vertices.Select(v => v.Id);
            foreach (var vertex in vertices)
            {
                var atom = molecule.GetAtom(vertex);
                var atom1Position = atom.Position;
                var atom1Radius = atom.Radius.Value;
                //var bondedNeighbors = new HashSet<Atom>(atom.OuterOrbitals
                //    .Where(o => o.IsPartOfBond)
                //    .Select(o => GetBondedAtom(o, atom)));

                var neighborhood = neighborhoodMap.GetNeighborhood(vertex);
                foreach (var neighborVertex in neighborhood)
                {
                    if (neighborVertex <= vertex)
                        continue;
                    var neighborAtom = molecule.GetAtom(neighborVertex);
                    //if (bondedNeighbors.Contains(neighborAtom))
                    //    continue;
                    var atom2Radius = atom.Radius.Value;
                    var atomRadiusSum = atom1Radius + atom2Radius;
                    var r = atom1Position.VectorTo(neighborAtom.Position);
                    var distance = r.Magnitude().In(SIPrefix.Pico, Unit.Meter);

                    var shellRepulsionForce = -(1e-5*(1e-12/distance)
                                                *Math.Exp(Math.Min(1e12*(atomRadiusSum - distance), 0)))
                                              *r.Normalize().ToVector3D();

                    forceLookup[vertex] += shellRepulsionForce;
                    forceLookup[neighborVertex] += -shellRepulsionForce;
                }
            }
        }

        private static Dictionary<Orbital, Vector3D> CalculateLonePairRepulsion(Graph<Atom,SimpleBond> graph, 
            IDictionary<uint, Vector3D> forceLookup)
        {
            var lonePairForceLookup = graph.Vertices
                .Select(v => (AtomWithOrbitals)v.Object)
                .SelectMany(atom => atom.LonePairs)
                .ToDictionary(o => o, o => new Vector3D(0,0,0));
            foreach(var vertex in graph.Vertices)
            {
                var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(graph, vertex).Cast<IVertex<Atom>>().ToList();
                var currentAtom = (AtomWithOrbitals)vertex.Object;
                var filledOuterOrbitals = currentAtom.OuterOrbitals.Where(o => o.IsFull).ToList();
                var orbitalNeighborVertexMap = MapBondOrbitalToNeighborVertex(
                    filledOuterOrbitals, currentAtom, adjacentVertices);
                for (var orbitalIdx1 = 0; orbitalIdx1 < filledOuterOrbitals.Count; orbitalIdx1++)
                {
                    var orbital1 = filledOuterOrbitals[orbitalIdx1];
                    for (var orbitalIdx2 = orbitalIdx1+1; orbitalIdx2 < filledOuterOrbitals.Count; orbitalIdx2++)
                    {
                        var orbital2 = filledOuterOrbitals[orbitalIdx2];
                        var repulsiveForce = CalculateRepulsiveForce(orbital1, orbital2);

                        var orbtial1Vector = currentAtom.Position
                            .VectorTo(orbital1.MaximumElectronDensityPosition)
                            .Normalize();
                        var orbital1ParallelForce = repulsiveForce.ProjectOnto(orbtial1Vector);
                        var orbital1RepulsiveForce = (repulsiveForce - orbital1ParallelForce).ToVector3D();

                        var orbtial2Vector = currentAtom.Position
                            .VectorTo(orbital2.MaximumElectronDensityPosition)
                            .Normalize();
                        var orbital2ParallelForce = repulsiveForce.ProjectOnto(orbtial2Vector);
                        var orbital2RepulsiveForce = (repulsiveForce - orbital2ParallelForce).ToVector3D();

                        if (orbital1.IsPartOfBond)
                        {
                            var neighborVertex = orbitalNeighborVertexMap[orbital1];
                            forceLookup[neighborVertex.Id] += orbital1RepulsiveForce;
                        }
                        else
                        {
                            lonePairForceLookup[orbital1] += orbital1RepulsiveForce;
                        }
                        if (orbital2.IsPartOfBond)
                        {
                            var neighborVertex = orbitalNeighborVertexMap[orbital2];
                            forceLookup[neighborVertex.Id] += -orbital2RepulsiveForce;
                        }
                        else
                        {
                            lonePairForceLookup[orbital2] += -orbital2RepulsiveForce;
                        }
                    }
                }
            }
            return lonePairForceLookup;
        }

        private static Dictionary<Orbital, IVertex<Atom>> MapBondOrbitalToNeighborVertex(
            List<Orbital> filledOuterOrbitals, AtomWithOrbitals currentAtom, List<IVertex<Atom>> adjacentVertices)
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

        private static Atom GetBondedAtom(Orbital orbital, Atom currentAtom)
        {
            if(!orbital.IsPartOfBond)
                throw new InvalidOperationException("Orbital is not part of bond. Cannot get bonded atom");
            var bondedAtom = orbital.AssociatedBond.Atom1.Equals(currentAtom)
                ? orbital.AssociatedBond.Atom2
                : orbital.AssociatedBond.Atom1;
            return bondedAtom;
        }

        private static Vector3D CalculateRepulsiveForce(Orbital orbital1, Orbital orbital2)
        {
            const double lonePairForceExpansion = 0.5;

            var distance = orbital1.MaximumElectronDensityPosition
                .DistanceTo(orbital2.MaximumElectronDensityPosition);
            if(distance == 0)
                return new Vector3D(0,0,0);
            var forceVector = orbital1.MaximumElectronDensityPosition
                .VectorTo(orbital2.MaximumElectronDensityPosition)
                .Normalize()
                .ToVector3D();
            var chargeProduct = PhysicalConstants.CoulombsConstant.Value
                *(2*PhysicalConstants.ElementaryCharge.Value)
                *(2*PhysicalConstants.ElementaryCharge.Value);
            if (!orbital1.IsPartOfBond)
            {
                if (orbital2.IsPartOfBond)
                    chargeProduct *= 1 + lonePairForceExpansion;
                else
                    chargeProduct *= 1 + 2*lonePairForceExpansion;
            }
            var repulsiveForce = -1e0*(chargeProduct / (distance*distance))*forceVector;
            return repulsiveForce;
        }
    }

    public class ForceCalculatorResult
    {
        public ForceCalculatorResult(Dictionary<uint, Vector3D> forceLookup, 
            Dictionary<Orbital, Vector3D> lonePairForceLookup, 
            AtomNeighborhoodMap neighborhoodMap)
        {
            ForceLookup = forceLookup;
            LonePairForceLookup = lonePairForceLookup;
            NeighborhoodMap = neighborhoodMap;
        }

        public Dictionary<uint, Vector3D> ForceLookup { get; }
        public Dictionary<Orbital, Vector3D> LonePairForceLookup { get; }
        public AtomNeighborhoodMap NeighborhoodMap { get; }
    }
}
