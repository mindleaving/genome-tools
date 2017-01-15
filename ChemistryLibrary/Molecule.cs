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

            var bond = AtomConnector.CreateBond(atom, existingAtom, bondMultiplicity);
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
            var edge = MoleculeStructure.ConnectVertices(
                moleculeReference.LastAtomId,
                mergeInfo.VertexIdMap[existingAtomId]);
            var atom1 = (Atom)MoleculeStructure.Vertices[edge.Vertex1Id].Object;
            var atom2 = (Atom)MoleculeStructure.Vertices[edge.Vertex2Id].Object;
            edge.Object = AtomConnector.CreateBond(atom1, atom2, bondMultiplicity);
        }

        public void UpdateBonds()
        {
            
        }

        public void PositionAtoms()
        {
            var unpositionedAtoms = new Queue<Vertex>(MoleculeStructure.Vertices.Values);
            while (unpositionedAtoms.Count > 0)
            {
                var vertex = unpositionedAtoms.Dequeue();

            }
        }

        public Atom GetAtom(uint atomId)
        {
            return (Atom) MoleculeStructure.Vertices[atomId].Object;
        }

        public void ConnectAtoms(uint atomId1, uint atomId2, BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            var atom1 = GetAtom(atomId1);
            var atom2 = GetAtom(atomId2);
            var edge = MoleculeStructure.ConnectVertices(atomId1, atomId2);
            var bond = AtomConnector.CreateBond(atom1, atom2, bondMultiplicity);
            edge.Object = bond;
        }
    }
}
