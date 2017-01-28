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

            var bond = AtomConnector.CreateBonds(atom, existingAtom, bondMultiplicity);
            var edge = new Edge(MoleculeStructure.GetUnusedEdgeId(), existingAtomId, vertex.Id)
            {
                Object = bond
            };
            MoleculeStructure.AddEdge(edge);
            return vertex.Id;
        }

        public void AddMolecule(MoleculeReference moleculeReference, uint existingAtomId, BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            var mergeInfo = MoleculeStructure.AddGraph(moleculeReference.Molecule.MoleculeStructure);
            var vertex1 = moleculeReference.LastAtomId;
            var vertex2 = mergeInfo.VertexIdMap[existingAtomId];
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

            var startVertex = MoleculeStructure.Vertices.Values.First();
            var startAtom = (Atom)startVertex.Object;
            startAtom.Position = new UnitPoint3D(Unit.Meter, 0,0,0);
            PositionNeighborAtoms(startVertex);
            var connectedVertices = GraphAlgorithms.GetConnectedSubgraph(MoleculeStructure, startVertex);
            foreach (var vertex in connectedVertices)
            {
                var neighborAtoms = GraphAlgorithms.GetAdjacentVertices(MoleculeStructure, vertex)
                    .Select(v => (Atom) v.Object);
                var positionedNeighbors = neighborAtoms.Where(atom => atom.Position != null).ToList();
                
                // Position current vertex



                // Position neighbors
                throw new NotImplementedException();
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
