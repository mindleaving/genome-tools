using System.Collections.Generic;

namespace ChemistryLibrary
{
    public class Peptide
    {
        public Peptide(MoleculeReference moleculeReference, List<AminoAcidReference> aminoAcids)
        {
            MoleculeReference = moleculeReference;
            AminoAcids = aminoAcids;
        }

        public Molecule Molecule => MoleculeReference.Molecule;
        public MoleculeReference MoleculeReference { get; }
        public List<AminoAcidReference> AminoAcids { get; }
    }

    public class AminoAcidReference : MoleculeReference
    {
        public AminoAcidReference(AminoAcidName aminoAcidName, MoleculeReference moleculeReference)
            : this(aminoAcidName, moleculeReference.Molecule, moleculeReference.FirstAtomId, moleculeReference.LastAtomId)
        {
        }

        public AminoAcidReference(AminoAcidName name, Molecule molecule, uint firstAtomId, uint lastAtomId) 
            : base(molecule, firstAtomId, lastAtomId)
        {
            Name = name;
        }

        public AminoAcidName Name { get; }

    }
}
