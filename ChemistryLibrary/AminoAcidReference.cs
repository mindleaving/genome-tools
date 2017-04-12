using System.Collections.Generic;
using System.Linq;

namespace ChemistryLibrary
{
    public class AminoAcidReference : MoleculeReference
    {
        public AminoAcidReference(AminoAcidName aminoAcidName, MoleculeReference moleculeReference)
            : this(aminoAcidName, moleculeReference.Molecule, moleculeReference.VertexIds, moleculeReference.FirstAtomId, moleculeReference.LastAtomId)
        {
        }

        public AminoAcidReference(AminoAcidName name, Molecule molecule, IEnumerable<uint> vertexIds,  uint firstAtomId, uint lastAtomId) 
            : base(molecule, vertexIds, firstAtomId, lastAtomId)
        {
            Name = name;
        }

        public AminoAcidName Name { get; }

        public Atom GetAtomFromName(string atomName)
        {
            return Molecule.Atoms.SingleOrDefault(atom => atom.AminoAcidAtomName == atomName);
        }
    }
}