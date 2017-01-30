using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
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
            CalculateIonicForces(molecule, forceLookup, neighborhoodMap);
            CalculateAtomShellRepulsion(molecule, forceLookup, neighborhoodMap);
            var lonePairForceLookup = CalculateLonePairRepulsion(graph, forceLookup);
            if(forceLookup.Values.Any(v => v.X.Value.IsNaN()))
                throw new Exception("Force cannot be 'NaN'");
            if (lonePairForceLookup.Values.Any(v => v.X.Value.IsNaN()))
                throw new Exception("Lone pair repulsion cannot be 'NaN'");
            return new ForceCalculatorResult(forceLookup, lonePairForceLookup, neighborhoodMap);
        }

        private static Dictionary<uint, UnitVector3D> CalculateBondForces(Graph graph)
        {
            var forceLookup = graph.Vertices.ToDictionary(
                kvp => kvp.Key, 
                kvp => new UnitVector3D(Unit.Newton, 0, 0, 0));

            // Bond force
            foreach (var edge in graph.Edges.Values)
            {
                var vertex1 = graph.Vertices[edge.Vertex1Id];
                var vertex2 = graph.Vertices[edge.Vertex2Id];
                var atom1 = (Atom) vertex1.Object;
                var atom2 = (Atom) vertex2.Object;
                var bond = (Bond) edge.Object;

                var v1V2Vector = atom1.Position.VectorTo(atom2.Position);
                var forceDirection = v1V2Vector.In(Unit.Meter).Normalize();
                var atomDistance = v1V2Vector.Magnitude();
                var forceStrength = -1e4*bond.BondEnergy.In(Unit.ElectronVolts)
                    *(atomDistance - 0.8*bond.BondLength).In(Unit.Meter)
                    .To(Unit.Newton);

                forceLookup[vertex1.Id] += forceStrength*forceDirection;
                forceLookup[vertex2.Id] += -forceStrength*forceDirection;
            }
            return forceLookup;
        }

        private static void CalculateIonicForces(Molecule molecule,
            Dictionary<uint, UnitVector3D> forceLookup,
            AtomNeighborhoodMap neighborhoodMap)
        {
            var vertices = molecule.MoleculeStructure.Vertices.Keys;
            foreach (var vertex in vertices)
            {
                var atom = molecule.GetAtom(vertex);
                var charge1 = atom.EffectiveCharge;
                var atom1PositionInPicoMeter = atom.Position.In(SIPrefix.Pico, Unit.Meter);

                var neighborhood = neighborhoodMap.GetNeighborhood(vertex);
                foreach (var neighborVertex in neighborhood)
                {
                    var neighborAtom = molecule.GetAtom(neighborVertex);
                    var charge2 = neighborAtom.EffectiveCharge;
                    var r = atom1PositionInPicoMeter
                        .VectorTo(neighborAtom.Position.In(SIPrefix.Pico, Unit.Meter));
                    var distance = r.Magnitude().To(SIPrefix.Pico, Unit.Meter);
                    var chargeProduct = PhysicalConstants.CoulombsConstant
                                        * charge1
                                        * charge2;
                    var ionicForce = -(chargeProduct / (distance * distance)) * r.Normalize();

                    forceLookup[vertex] += ionicForce;
                    forceLookup[neighborVertex] += -ionicForce;
                }
            }
        }

        private static void CalculateAtomShellRepulsion(Molecule molecule,
            Dictionary<uint, UnitVector3D> forceLookup,
            AtomNeighborhoodMap neighborhoodMap)
        {
            var vertices = molecule.MoleculeStructure.Vertices.Keys;
            foreach (var vertex in vertices)
            {
                var atom = molecule.GetAtom(vertex);
                var atom1PositionInPicoMeter = atom.Position.In(SIPrefix.Pico, Unit.Meter);
                var atom1Radius = atom.Radius.In(SIPrefix.Pico, Unit.Meter);
                var bondedNeighbors = new HashSet<Atom>(atom.OuterOrbitals
                    .Where(o => o.IsPartOfBond)
                    .Select(o => GetBondedAtom(o, atom)));

                var neighborhood = neighborhoodMap.GetNeighborhood(vertex);
                foreach (var neighborVertex in neighborhood)
                {
                    var neighborAtom = molecule.GetAtom(neighborVertex);
                    if (bondedNeighbors.Contains(neighborAtom))
                        continue;
                    var atom2Radius = atom.Radius.In(SIPrefix.Pico, Unit.Meter);
                    var atomRadiusSum = atom1Radius + atom2Radius;
                    var r = atom1PositionInPicoMeter
                        .VectorTo(neighborAtom.Position.In(SIPrefix.Pico, Unit.Meter));
                    var distanceInPicoMeter = r.Magnitude();

                    var shellRepulsionForce = -(1e-6*(1.0/distanceInPicoMeter)
                        * Math.Exp(Math.Min(atomRadiusSum - distanceInPicoMeter,0)))
                        .To(Unit.Newton) 
                        * r.Normalize();

                    if(shellRepulsionForce.Magnitude() > 1e-5.To(Unit.Newton))
                        continue;

                    forceLookup[vertex] += shellRepulsionForce;
                    forceLookup[neighborVertex] += -shellRepulsionForce;
                }
            }
        }

        private static Dictionary<Orbital, UnitVector3D> CalculateLonePairRepulsion(Graph graph, 
            IDictionary<uint, UnitVector3D> forceLookup)
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

        private static Dictionary<Orbital, Vertex> MapBondOrbitalToNeighborVertex(
            List<Orbital> filledOuterOrbitals, Atom currentAtom, List<Vertex> adjacentVertices)
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
            var bondedAtom = orbital.AssociatedBond.Atom1 == currentAtom
                ? orbital.AssociatedBond.Atom2
                : orbital.AssociatedBond.Atom1;
            return bondedAtom;
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
            return repulsiveForce;
        }
    }

    public class ForceCalculatorResult
    {
        public ForceCalculatorResult(Dictionary<uint, UnitVector3D> forceLookup, 
            Dictionary<Orbital, UnitVector3D> lonePairForceLookup, 
            AtomNeighborhoodMap neighborhoodMap)
        {
            ForceLookup = forceLookup;
            LonePairForceLookup = lonePairForceLookup;
            NeighborhoodMap = neighborhoodMap;
        }

        public Dictionary<uint, UnitVector3D> ForceLookup { get; }
        public Dictionary<Orbital, UnitVector3D> LonePairForceLookup { get; }
        public AtomNeighborhoodMap NeighborhoodMap { get; }
    }
}
