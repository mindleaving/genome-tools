using System.Collections.Generic;

namespace ChemistryLibrary
{
    public class Peptide
    {
        public Peptide(MoleculeReference moleculeReference,
            List<AminoAcidReference> aminoAcids)
            : this(moleculeReference, aminoAcids, new List<PeptideAnnotation>())
        {
        }
        public Peptide(MoleculeReference moleculeReference, 
            List<AminoAcidReference> aminoAcids,
            List<PeptideAnnotation> annotations)
        {
            MoleculeReference = moleculeReference;
            AminoAcids = aminoAcids;
            Annotations = annotations;
        }

        public char ChainId { get; set; }
        public Molecule Molecule => MoleculeReference.Molecule;
        public MoleculeReference MoleculeReference { get; }
        public List<AminoAcidReference> AminoAcids { get; }
        public List<PeptideAnnotation> Annotations { get; }
    }

    public class PeptideAnnotation
    {
        public PeptideAnnotation(PeptideSecondaryStructure type,
            List<AminoAcidReference> aminoAcidReferences,
            int firstAminoAcidNumber)
        {
            Type = type;
            FirstAminoAcidNumber = firstAminoAcidNumber;
            AminoAcidReferences.AddRange(aminoAcidReferences);
        }

        public PeptideSecondaryStructure Type { get; }
        public int FirstAminoAcidNumber { get; }
        public List<AminoAcidReference> AminoAcidReferences { get; } = new List<AminoAcidReference>();
    }

    public enum PeptideSecondaryStructure
    {
        None,
        AlphaHelix,
        BetaSheet
    }
}
