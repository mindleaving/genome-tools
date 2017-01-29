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

        public void AddMolecule(MoleculeReference moleculeReference, uint existingAtomId, BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            var mergeInfo = MoleculeStructure.AddGraph(moleculeReference.Molecule.MoleculeStructure);
            var vertex1 = existingAtomId;
            var vertex2 = mergeInfo.VertexIdMap[moleculeReference.LastAtomId];
            var atom1 = (Atom)MoleculeStructure.Vertices[vertex1].Object;
            var atom2 = (Atom)MoleculeStructure.Vertices[vertex2].Object;
            var bonds = AtomConnector.CreateBonds(atom1, atom2, bondMultiplicity);
            foreach (var bond in bonds)
            {
                var edge = MoleculeStructure.ConnectVertices(vertex1, vertex2);
                edge.Object = bond;
            }
        }

        public void UpdateBonds()
        {
            throw new NotImplementedException();
        }

        public void PositionAtoms()
        {
            MoleculeStructure.Vertices.Values.ForEach(v => ((Atom)v.Object).Position = null);

            var positionableVertices = new Queue<Vertex>();

            var startVertex = MoleculeStructure.Vertices.Values.First();
            positionableVertices.Enqueue(startVertex);
            var startAtom = (Atom)startVertex.Object;
            startAtom.Position = new UnitPoint3D(Unit.Meter, 0,0,0);
            while (positionableVertices.Any())
            {
                var vertex = positionableVertices.Dequeue();
                var vertextEdges = vertex.EdgeIds.Select(edgeId => MoleculeStructure.Edges[edgeId]).ToList();
                var currentAtom = (Atom) vertex.Object;
                var lonePairs = currentAtom.OuterOrbitals.Where(o => o.IsFull && !o.IsPartOfBond).ToList();

                var adjacentVertices = GraphAlgorithms.GetAdjacentVertices(MoleculeStructure, vertex).ToList();
                var positionedNeighbors = adjacentVertices.Where(v => ((Atom) v.Object).Position != null).ToList();
                var unpositionedNeighbors = adjacentVertices.Where(v => ((Atom)v.Object).Position == null).ToList();

                var existingPoints = positionedNeighbors
                        .Select(v => (Atom)v.Object)
                        .Select(a => currentAtom.Position.VectorTo(a.Position))
                        .ToList();
                var neighborAndLonePairCount = currentAtom.OuterOrbitals.Count(o => o.IsFull);
                var evenlyDistributedPoints = SpherePointDistributor.EvenlyDistributePointsOnSphere(1.0, neighborAndLonePairCount - existingPoints.Count, existingPoints);
                var evenlySpacePointsQueue = new Queue<Point3D>(evenlyDistributedPoints);

                foreach (var neighbor in unpositionedNeighbors)
                {
                    var atom = (Atom) neighbor.Object;
                    if(atom.Position != null)
                        continue;
                    positionableVertices.Enqueue(neighbor);

                    var connectingEdges = vertextEdges.Where(edge => edge.HasVertex(neighbor.Id)).ToList();
                    var bonds = connectingEdges.Select(edge => (Bond) edge.Object);
                    var bondLength = bonds.Average(bond => bond.BondLength, SIPrefix.Pico, Unit.Meter);
                    var bondDirection = evenlySpacePointsQueue.Dequeue().ToVector3D();
                    var neighborPosition = currentAtom.Position + bondLength.In(SIPrefix.Pico, Unit.Meter)*bondDirection;
                    atom.Position = neighborPosition.To(SIPrefix.Pico, Unit.Meter);
                }
                foreach (var lonePair in lonePairs)
                {
                    var lonePairDirection = evenlySpacePointsQueue.Dequeue().ToVector3D();

                    var lonePairPosition = currentAtom.Position 
                        + currentAtom.Radius.In(SIPrefix.Pico, Unit.Meter)*lonePairDirection;
                    lonePair.MaximumElectronDensityPosition = lonePairPosition;
                }
            }
        }

        private void PositionNeighborAtoms(Vertex startVertex)
        {
            throw new NotImplementedException();
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
