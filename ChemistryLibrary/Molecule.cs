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

        public uint AddFirstAtom(Atom atom)
        {
            var vertex = new Vertex(MoleculeStructure.GetUnusedVertexId())
            {
                Object = atom
            };
            MoleculeStructure.AddVertex(vertex);
            return vertex.Id;
        }

        public uint AddAtom(Atom atom, uint existingAtomId, Bond bond = null)
        {
            if(!MoleculeStructure.Vertices.ContainsKey(existingAtomId))
                throw new KeyNotFoundException("Adding atom to molecule failed, because the reference to an existing atom was not found");
            var existingAtom = (Atom)MoleculeStructure.Vertices[existingAtomId].Object;
            if (bond != null
                && ((bond.Atom1 != atom && bond.Atom2 != atom) || (bond.Atom1 != existingAtom && bond.Atom2 != existingAtom)))
            {
                throw new ArgumentException("Provided bond doesn't concern one or either of the two atoms being connected");
            }
            var vertex = new Vertex(MoleculeStructure.GetUnusedVertexId())
            {
                Object = atom
            };
            MoleculeStructure.AddVertex(vertex);

            if (bond == null)
            {
                bond = AtomConnector.CreateBond(existingAtom, atom);
            }
            var edge = new Edge(MoleculeStructure.GetUnusedEdgeId(), existingAtomId, vertex.Id)
            {
                Object = bond
            };
            MoleculeStructure.AddEdge(edge);
            return vertex.Id;
        }
    }
}
