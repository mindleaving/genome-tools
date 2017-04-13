using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public static class MoleculePositioner
    {
        public static void PositionAtoms(Molecule molecule, uint firstAtomId = uint.MaxValue, uint lastAtomId = uint.MaxValue)
        {
            // Remove position information
            molecule.MoleculeStructure.Vertices.Values
                .Select(v => (Atom)v.Object)
                .Where(atom => !atom.IsPositionFixed)
                .ForEach(atom => atom.Position = null);

            var positionableVertices = new Queue<Vertex>();

            // If first and last atom is specified, position atoms between those two first
            // Usually the case when a peptide is positioned, in which case the backbone
            // is oriented along the X-axis
            if (firstAtomId != uint.MaxValue && lastAtomId != uint.MaxValue)
            {
                // Position first atom
                var firstVertex = molecule.MoleculeStructure.Vertices[firstAtomId];
                var firstAtom = (Atom) firstVertex.Object;
                if(firstAtom.Position == null)
                    firstAtom.Position = new UnitPoint3D(Unit.Meter, 0,0,0);
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
                var startAtom = (Atom)startVertex.Object;
                if(startAtom.Position == null)
                    startAtom.Position = new UnitPoint3D(Unit.Meter, 0, 0, 0);
            }
            while (positionableVertices.Any())
            {
                var vertex = positionableVertices.Dequeue();
                var positionableNeighbors = PositionNeighborsAndLonePairs(molecule, vertex);
                positionableNeighbors.ForEach(neighbor => positionableVertices.Enqueue(neighbor));
            }
        }

        private static IEnumerable<Vertex> PositionNeighborsAndLonePairs(Molecule molecule, Vertex vertex)
        {
            var currentAtom = (Atom) vertex.Object;

            var vertextEdges = vertex.EdgeIds.Select(edgeId => molecule.MoleculeStructure.Edges[edgeId]).ToList();
            var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(molecule.MoleculeStructure, vertex).ToList();
            var unpositionedNeighbors = adjacentVertices.Where(v => ((Atom) v.Object).Position == null).ToList();

            var evenlyDistributedPoints = GetAtomSpherePoints(currentAtom, adjacentVertices);
            var evenlySpacePointsQueue = new Queue<Point3D>(evenlyDistributedPoints);

            var neighborsReadyForPositioning = new List<Vertex>();
            foreach (var neighbor in unpositionedNeighbors)
            {
                var atom = (Atom) neighbor.Object;
                if (atom.Position != null)
                    continue;
                neighborsReadyForPositioning.Add(neighbor);

                var connectingEdges = vertextEdges.Where(edge => edge.HasVertex(neighbor.Id)).ToList();
                var bonds = connectingEdges.Select(edge => (Bond) edge.Object);
                var bondLength = bonds.Select(bond => bond.BondLength.Value).Average();
                var bondDirection = evenlySpacePointsQueue.Dequeue().ToVector3D();
                var neighborPosition = currentAtom.Position + bondLength*bondDirection;
                atom.Position = neighborPosition.To(SIPrefix.Pico, Unit.Meter);
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

        private static void PositionSpecificNeighborAlongXAxis(Molecule molecule, Vertex currentVertex, Vertex neighborVertex)
        {
            var currentAtom = (Atom)currentVertex.Object;
            var neighborAtom = (Atom) neighborVertex.Object;
            if(neighborAtom.Position != null)
                return;
            var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(molecule.MoleculeStructure, currentVertex).ToList();
            var evenlyDistributedPoints = GetAtomSpherePoints(currentAtom, adjacentVertices);
            var edgesToNeighbor = currentVertex.EdgeIds
                .Select(edgeId => molecule.MoleculeStructure.Edges[edgeId])
                .Where(edge => edge.HasVertex(neighborVertex))
                .ToList();
            if(!edgesToNeighbor.Any())
                throw new ArgumentException("Vertex to be positioned is not a neighbor to the current vertex");

            var bonds = edgesToNeighbor.Select(edge => (Bond)edge.Object);
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
        }

        private static IEnumerable<Point3D> GetAtomSpherePoints(Atom currentAtom, IEnumerable<Vertex> adjacentVertices)
        {
            var positionedNeighbors = adjacentVertices.Where(v => ((Atom) v.Object).Position != null).ToList();
            var existingPoints = positionedNeighbors
                .Select(v => (Atom) v.Object)
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