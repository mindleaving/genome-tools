using System;
using System.Collections.Generic;

namespace ChemistryLibrary.Objects
{
    public class Peptide : IDisposable
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
            MoleculeReference = moleculeReference ?? throw new ArgumentNullException(nameof(moleculeReference));
            AminoAcids = aminoAcids;
            Annotations = annotations;
        }

        public char ChainId { get; set; }
        public Molecule Molecule => MoleculeReference.Molecule;
        public MoleculeReference MoleculeReference { get; }
        public List<AminoAcidReference> AminoAcids { get; }
        public List<PeptideAnnotation> Annotations { get; }

        public void Dispose()
        {
            MoleculeReference.Dispose();
            AminoAcids.ForEach(aminoAcid => aminoAcid.Dispose());
            Annotations.ForEach(annotation => annotation.Dispose());
            AminoAcids.Clear();
            Annotations.Clear();
        }
    }
}
