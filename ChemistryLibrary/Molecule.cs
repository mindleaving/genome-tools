using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public class Molecule
    {
        public IEnumerable<Atom> Atoms => MoleculeStructure.Vertices.Values.Select(v => (Atom) v.Object);
        public Graph MoleculeStructure { get; } = new Graph();
        public UnitValue Charge => Atoms.Sum(atom => atom.FormalCharge, Unit.ElementaryCharge);
        public bool IsPositioned { get; private set; }

        public uint AddAtom(Atom atom, uint existingAtomId = uint.MaxValue, BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            // If no atoms in the molecule yet, just add the atom
            if (!MoleculeStructure.Vertices.Any())
            {
                var firstVertex = new Vertex(MoleculeStructure.GetUnusedVertexId())
                {
                    Object = atom
                };
                MoleculeStructure.AddVertex(firstVertex);
                return firstVertex.Id;
            }

            if(!MoleculeStructure.Vertices.ContainsKey(existingAtomId))
                throw new KeyNotFoundException("Adding atom to molecule failed, because the reference to an existing atom was not found");
            var existingAtom = (Atom)MoleculeStructure.Vertices[existingAtomId].Object;
            var vertex = new Vertex(MoleculeStructure.GetUnusedVertexId())
            {
                Object = atom
            };
            MoleculeStructure.AddVertex(vertex);

            var bonds = AtomConnector.CreateBonds(atom, existingAtom, bondMultiplicity);
            foreach (var bond in bonds)
            {
                var edge = new Edge(MoleculeStructure.GetUnusedEdgeId(), existingAtomId, vertex.Id)
                {
                    Object = bond
                };
                MoleculeStructure.AddEdge(edge);
            }
            return vertex.Id;
        }

        public MoleculeReference AddMolecule(MoleculeReference moleculeToBeAdded, 
            uint firstAtomId,
            uint connectionAtomId, 
            BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            var mergeInfo = MoleculeStructure.AddGraph(moleculeToBeAdded.Molecule.MoleculeStructure);
            var vertex1 = connectionAtomId;
            var vertex2 = mergeInfo.VertexIdMap[moleculeToBeAdded.FirstAtomId];
            var atom1 = (Atom)MoleculeStructure.Vertices[vertex1].Object;
            var atom2 = (Atom)MoleculeStructure.Vertices[vertex2].Object;
            var bonds = AtomConnector.CreateBonds(atom1, atom2, bondMultiplicity);
            foreach (var bond in bonds)
            {
                var edge = MoleculeStructure.ConnectVertices(vertex1, vertex2);
                edge.Object = bond;
            }
            return new MoleculeReference(this, firstAtomId, mergeInfo.VertexIdMap[moleculeToBeAdded.LastAtomId]);
        }

        public void UpdateBonds()
        {
            throw new NotImplementedException();
        }

        public void PositionAtoms(uint firstAtomId = uint.MaxValue, uint lastAtomId = uint.MaxValue)
        {
            MoleculeStructure.Vertices.Values.ForEach(v => ((Atom)v.Object).Position = null);

            var positionableVertices = new Queue<Vertex>();

            if (firstAtomId != uint.MaxValue && lastAtomId != uint.MaxValue)
            {
                var firstVertex = MoleculeStructure.Vertices[firstAtomId];
                var lastVertex = MoleculeStructure.Vertices[lastAtomId];
                var pathFromFirstVertex = GraphAlgorithms.ShortestPaths(MoleculeStructure, firstVertex);
                var pathToLastVertex = pathFromFirstVertex.PathTo(lastVertex);
                // TODO
            }
            else
            {
                var startVertex = firstAtomId != uint.MaxValue
                    ? MoleculeStructure.Vertices[firstAtomId]
                    : MoleculeStructure.Vertices.Values.First();
                positionableVertices.Enqueue(startVertex);
                var startAtom = (Atom)startVertex.Object;
                startAtom.Position = new Point3D(0, 0, 0);
            }
            while (positionableVertices.Any())
            {
                var vertex = positionableVertices.Dequeue();
                var positionableNeighbors = PositionNeighborsAndLonePairs(vertex);
                positionableNeighbors.ForEach(neighbor => positionableVertices.Enqueue(neighbor));
            }
            IsPositioned = true;
        }

        private IEnumerable<Vertex> PositionNeighborsAndLonePairs(Vertex vertex)
        {
            var currentAtom = (Atom) vertex.Object;

            var vertextEdges = vertex.EdgeIds.Select(edgeId => MoleculeStructure.Edges[edgeId]).ToList();
            var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(MoleculeStructure, vertex).ToList();
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
                atom.Position = neighborPosition;
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

        public void PositionSpecificNeighborAlongXAxis(Vertex currentVertex, Vertex neighborVertex)
        {
            var currentAtom = (Atom)currentVertex.Object;
            var neighborAtom = (Atom) neighborVertex.Object;
            if(neighborAtom.Position != null)
                return;
            var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(MoleculeStructure, currentVertex).ToList();
            var evenlyDistributedPoints = GetAtomSpherePoints(currentAtom, adjacentVertices);
            var edgesToNeighbor = currentVertex.EdgeIds
                .Select(edgeId => MoleculeStructure.Edges[edgeId])
                .Where(edge => edge.HasVertex(neighborVertex))
                .ToList();
            if(edgesToNeighbor == null)
                throw new ArgumentException("Vertex to be positioned is not a neighbor to the current vertex");

            var bonds = edgesToNeighbor.Select(edge => (Bond)edge.Object);
            var bondLength = bonds.Select(bond => bond.BondLength.Value).Average();
            var candidateAtomPositions = evenlyDistributedPoints
                .Select(bondDirection => currentAtom.Position + bondLength*bondDirection);
            var positionClosestToXAxis = candidateAtomPositions
                .MinimumItem(p => )

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

        public Atom GetAtom(uint atomId)
        {
            return (Atom) MoleculeStructure.Vertices[atomId].Object;
        }

        public void ConnectAtoms(uint atomId1, uint atomId2, BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            var atom1 = GetAtom(atomId1);
            var atom2 = GetAtom(atomId2);
            var bonds = AtomConnector.CreateBonds(atom1, atom2, bondMultiplicity);
            foreach (var bond in bonds)
            {
                var edge = MoleculeStructure.ConnectVertices(atomId1, atomId2);
                edge.Object = bond;
            }
        }
    }
}
