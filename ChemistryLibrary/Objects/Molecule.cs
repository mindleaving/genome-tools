using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class Molecule : IDisposable
    {
        public IEnumerable<Atom> Atoms => MoleculeStructure.Vertices.Select(v => v.Object);
        public Graph<Atom,SimpleBond> MoleculeStructure { get; } = new Graph<Atom,SimpleBond>();
        public UnitValue Charge => Atoms.Sum(atom => atom.FormalCharge, Unit.ElementaryCharge);
        public bool IsPositioned { get; private set; }

        public uint AddAtom(Atom atom, uint existingAtomId = uint.MaxValue, BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            // If no atoms in the molecule yet, just add the atom
            if (!MoleculeStructure.Vertices.Any())
            {
                var firstVertex = new Vertex<Atom>(MoleculeStructure.GetUnusedVertexId())
                {
                    Object = atom
                };
                MoleculeStructure.AddVertex(firstVertex);
                return firstVertex.Id;
            }

            if(!MoleculeStructure.HasVertex(existingAtomId))
                throw new KeyNotFoundException("Adding atom to molecule failed, because the reference to an existing atom was not found");
            var existingAtom = MoleculeStructure.GetVertexFromId(existingAtomId).Object;
            var vertex = new Vertex<Atom>(MoleculeStructure.GetUnusedVertexId())
            {
                Object = atom
            };
            MoleculeStructure.AddVertex(vertex);

            var bonds = AtomConnector.CreateBonds(atom, existingAtom, bondMultiplicity);
            foreach (var bond in bonds)
            {
                var edge = new Edge<SimpleBond>(MoleculeStructure.GetUnusedEdgeId(), existingAtomId, vertex.Id)
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
            return AddMolecule(moleculeToBeAdded, firstAtomId, connectionAtomId, out _, bondMultiplicity);
        }

        public MoleculeReference AddMolecule(MoleculeReference moleculeToBeAdded, 
            uint firstAtomId,
            uint connectionAtomId, 
            out MoleculeReference convertedInputMoleculeReference,
            BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            var mergeInfo = MoleculeStructure.AddGraph(moleculeToBeAdded.Molecule.MoleculeStructure);
            var vertex1 = connectionAtomId;
            var vertex2 = mergeInfo.VertexIdMap[moleculeToBeAdded.FirstAtomId];
            var atom1 = MoleculeStructure.GetVertexFromId(vertex1).Object;
            var atom2 = MoleculeStructure.GetVertexFromId(vertex2).Object;
            var bonds = AtomConnector.CreateBonds(atom1, atom2, bondMultiplicity);
            foreach (var bond in bonds)
            {
                var edge = (Edge<SimpleBond>)MoleculeStructure.ConnectVertices(vertex1, vertex2);
                edge.Object = bond;
            }
            convertedInputMoleculeReference = new MoleculeReference(this,
                moleculeToBeAdded.VertexIds.Select(oldVertexId => mergeInfo.VertexIdMap[oldVertexId]),
                mergeInfo.VertexIdMap[moleculeToBeAdded.FirstAtomId],
                mergeInfo.VertexIdMap[moleculeToBeAdded.LastAtomId]);
            return new MoleculeReference(this, firstAtomId, mergeInfo.VertexIdMap[moleculeToBeAdded.LastAtomId]);
        }

        public void UpdateBonds()
        {
            throw new NotImplementedException();
        }

        public void PositionAtoms(uint firstAtomId = uint.MaxValue, uint lastAtomId = uint.MaxValue)
        {
            MoleculePositioner.PositionAtoms(this, firstAtomId, lastAtomId);
            IsPositioned = true;
        }

        public Atom GetAtom(uint atomId)
        {
            return MoleculeStructure.GetVertexFromId(atomId).Object;
        }

        public void ConnectAtoms(uint atomId1, uint atomId2, BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            var atom1 = GetAtom(atomId1);
            var atom2 = GetAtom(atomId2);
            var bonds = AtomConnector.CreateBonds(atom1, atom2, bondMultiplicity);
            foreach (var bond in bonds)
            {
                var edge = (Edge<SimpleBond>)MoleculeStructure.ConnectVertices(atomId1, atomId2);
                edge.Object = bond;
            }
        }

        public void Dispose()
        {
            MoleculeStructure.Dispose();
        }
    }
}
