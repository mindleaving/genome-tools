using System;
using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;

namespace ChemistryLibrary.Builders
{
    public static class MoleculePositioner
    {
        public static void PositionAtoms(Molecule molecule, uint firstAtomId = uint.MaxValue, uint lastAtomId = uint.MaxValue)
        {
            // Remove position information
            molecule.MoleculeStructure.Vertices.Values
                .Select(v => v.Object)
                .ForEach(atom => atom.IsPositioned = false);

            var positionableVertices = new Queue<Vertex<Atom>>();

            // If first and last atom is specified, position atoms between those two first
            // Usually the case when a peptide is positioned, in which case the backbone
            // is oriented along the X-axis
            if (firstAtomId != uint.MaxValue && lastAtomId != uint.MaxValue)
            {
                // Position first atom
                var firstVertex = molecule.MoleculeStructure.Vertices[firstAtomId];
                var firstAtom = firstVertex.Object;
                if (firstAtom.Position == null)
                    firstAtom.Position = new UnitPoint3D(Unit.Meter, 0, 0, 0);
                firstAtom.IsPositioned = true;
                positionableVertices.Enqueue(firstVertex);

                // Trace through molecule to last atom
                var lastVertex = molecule.MoleculeStructure.Vertices[lastAtomId];
                var pathFromFirstVertex = GraphAlgorithms.ShortestPaths(molecule.MoleculeStructure, firstVertex);
                var pathToLastVertex = pathFromFirstVertex.PathTo(lastVertex);

                // Position atoms between first and last along X-axis
                var currentEdge = pathToLastVertex.Path.First;
                var currentVertex = firstVertex;
                while (currentEdge != null)
                {
                    var neighborId = currentEdge.Value.Vertex1Id == currentVertex.Id
                        ? currentEdge.Value.Vertex2Id
                        : currentEdge.Value.Vertex1Id;
                    var neighborVertex = molecule.MoleculeStructure.Vertices[neighborId];
                    PositionSpecificNeighborAlongXAxis(molecule, currentVertex, neighborVertex);
                    positionableVertices.Enqueue(neighborVertex);
                    currentEdge = currentEdge.Next;
                    currentVertex = neighborVertex;
                }
            }
            else
            {
                var startVertex = firstAtomId != uint.MaxValue
                    ? molecule.MoleculeStructure.Vertices[firstAtomId]
                    : molecule.MoleculeStructure.Vertices.Values.First();
                positionableVertices.Enqueue(startVertex);
                var startAtom = startVertex.Object;
                if (startAtom.Position == null)
                    startAtom.Position = new UnitPoint3D(Unit.Meter, 0, 0, 0);
                startAtom.IsPositioned = true;
            }
            while (positionableVertices.Any())
            {
                var vertex = positionableVertices.Dequeue();
                var positionableNeighbors = PositionNeighborsAndLonePairs(molecule, vertex);
                positionableNeighbors.ForEach(neighbor => positionableVertices.Enqueue(neighbor));
            }
        }

        private static IEnumerable<Vertex<Atom>> PositionNeighborsAndLonePairs(Molecule molecule, Vertex<Atom> vertex)
        {
            var currentAtom = (AtomWithOrbitals)vertex.Object;

            var vertextEdges = vertex.EdgeIds.Select(edgeId => molecule.MoleculeStructure.Edges[edgeId]).ToList();
            var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(molecule.MoleculeStructure, vertex).ToList();
            var unpositionedNeighbors = adjacentVertices.Where(v => !v.Object.IsPositioned).ToList();

            var evenlyDistributedPoints = GetAtomSpherePoints(currentAtom, adjacentVertices);
            var evenlySpacePointsQueue = new Queue<Point3D>(evenlyDistributedPoints);

            var neighborsReadyForPositioning = new List<Vertex<Atom>>();
            foreach (var neighbor in unpositionedNeighbors)
            {
                var atom = neighbor.Object;
                if (atom.IsPositioned)
                    continue;
                neighborsReadyForPositioning.Add(neighbor);
                if(atom.IsPositionFixed)
                {
                    atom.IsPositioned = true;
                    continue;
                }
                var connectingEdges = vertextEdges.Where(edge => edge.HasVertex(neighbor.Id)).ToList();
                var bonds = connectingEdges.Select(edge => (OrbitalBond) edge.Object);
                var bondLength = bonds.Select(bond => bond.BondLength.Value).Average();
                var bondDirection = evenlySpacePointsQueue.Dequeue().ToVector3D();
                var neighborPosition = currentAtom.Position + bondLength*bondDirection;
                atom.Position = neighborPosition.To(SIPrefix.Pico, Unit.Meter);
                atom.IsPositioned = true;
            }

            var lonePairs = currentAtom.OuterOrbitals.Where(o => o.IsFull && !o.IsPartOfBond).ToList();
            foreach (var lonePair in lonePairs)
            {
                var lonePairDirection = evenlySpacePointsQueue.Dequeue().ToVector3D();

                var lonePairPosition = currentAtom.Position
                                       + currentAtom.Radius.Value*lonePairDirection;
                lonePair.MaximumElectronDensityPosition = lonePairPosition;
            }
            return neighborsReadyForPositioning;
        }

        private static void PositionSpecificNeighborAlongXAxis(Molecule molecule, Vertex<Atom> currentVertex, Vertex<Atom> neighborVertex)
        {
            var currentAtom = currentVertex.Object;
            var neighborAtom = neighborVertex.Object;
            if(neighborAtom.IsPositioned)
                return;
            if (neighborAtom.IsPositionFixed)
            {
                neighborAtom.IsPositioned = true;
                return;
            }
            var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(molecule.MoleculeStructure, currentVertex).ToList();
            var evenlyDistributedPoints = GetAtomSpherePoints((AtomWithOrbitals)currentAtom, adjacentVertices);
            var edgesToNeighbor = currentVertex.EdgeIds
                .Select(edgeId => molecule.MoleculeStructure.Edges[edgeId])
                .Where(edge => edge.HasVertex(neighborVertex.Id))
                .ToList();
            if(!edgesToNeighbor.Any())
                throw new ArgumentException("Vertex to be positioned is not a neighbor of the current vertex");

            var bonds = edgesToNeighbor.Select(edge => (OrbitalBond)edge.Object);
            var bondLength = bonds.Select(bond => bond.BondLength.Value).Average();
            var candidateAtomPositions = evenlyDistributedPoints
                .Select(bondDirection => currentAtom.Position + bondLength*bondDirection)
                .ToList();
            var pointsFurtherAlongX = candidateAtomPositions.Where(p => p.X > currentAtom.Position.X).ToList();
            Point3D neighborPosition;
            if (pointsFurtherAlongX.Any())
            {
                neighborPosition = pointsFurtherAlongX
                    .MinimumItem(p => p.DistanceToLine(new Point3D(0, 0, 0), new Point3D(1, 0, 0)));
            }
            else
            {
                neighborPosition = candidateAtomPositions
                    .MinimumItem(p => p.DistanceToLine(new Point3D(0, 0, 0), new Point3D(1, 0, 0)));
            }
            neighborAtom.Position = neighborPosition.To(SIPrefix.Pico, Unit.Meter);
            neighborAtom.IsPositioned = true;
        }

        private static IEnumerable<Point3D> GetAtomSpherePoints(AtomWithOrbitals currentAtom, IEnumerable<Vertex<Atom>> adjacentVertices)
        {
            var positionedNeighbors = adjacentVertices
                .Where(v => v.Object.IsPositioned || v.Object.IsPositionFixed)
                .ToList();
            var existingPoints = positionedNeighbors
                .Select(v => v.Object)
                .Select(a => currentAtom.Position.VectorTo(a.Position))
                .ToList();
            var neighborAndLonePairCount = currentAtom.OuterOrbitals.Count(o => o.IsFull);
            var evenlyDistributedPoints = SpherePointDistributor.EvenlyDistributePointsOnSphere(
                1.0,
                neighborAndLonePairCount - existingPoints.Count,
                existingPoints);
            return evenlyDistributedPoints;
        }
    }
}