using System.Collections.Generic;
using System.Linq;

namespace ChemistryLibrary.Objects
{
    public class AminoAcidReference : MoleculeReference
    {
        public AminoAcidReference(AminoAcidName aminoAcidName, int sequenceNumber, MoleculeReference moleculeReference)
            : this(aminoAcidName, sequenceNumber, moleculeReference.Molecule, moleculeReference.VertexIds, moleculeReference.FirstAtomId, moleculeReference.LastAtomId)
        {
        }

        public AminoAcidReference(AminoAcidName name, int sequenceNumber,  Molecule molecule, IEnumerable<uint> vertexIds, uint firstAtomId, uint lastAtomId) 
            : base(molecule, vertexIds, firstAtomId, lastAtomId)
        {
            Name = name;
            SequenceNumber = sequenceNumber;
        }

        public AminoAcidName Name { get; }
        public int SequenceNumber { get; }

        public Atom GetAtomFromName(string atomName)
        {
            return VertexIds.Select(Molecule.GetAtom).SingleOrDefault(atom => atom.AminoAcidAtomName == atomName);
        }
    }
}