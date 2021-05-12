using System;
using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.Extensions;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class Peptide : IDisposable
    {
        public Peptide(MoleculeReference moleculeReference,
            List<AminoAcidReference> aminoAcids)
            : this(moleculeReference, aminoAcids, new List<PeptideAnnotation<AminoAcidReference>>())
        {
        }
        public Peptide(MoleculeReference moleculeReference, 
            List<AminoAcidReference> aminoAcids,
            List<PeptideAnnotation<AminoAcidReference>> annotations)
        {
            MoleculeReference = moleculeReference ?? throw new ArgumentNullException(nameof(moleculeReference));
            AminoAcids = aminoAcids;
            Annotations = annotations;
        }

        public char ChainId { get; set; }
        public Molecule Molecule => MoleculeReference.Molecule;
        public MoleculeReference MoleculeReference { get; }
        public List<AminoAcidReference> AminoAcids { get; }
        public List<PeptideAnnotation<AminoAcidReference>> Annotations { get; }

        public void Dispose()
        {
            MoleculeReference.Dispose();
            AminoAcids.ForEach(aminoAcid => aminoAcid.Dispose());
            Annotations.ForEach(annotation => annotation.Dispose());
            AminoAcids.Clear();
            Annotations.Clear();
        }

        public void Add(AminoAcidReference aminoAcid)
        {
            MoleculeReference.Add(aminoAcid, out var aminoAcidReference);
            AminoAcids.Add(new AminoAcidReference(aminoAcid.Name, aminoAcid.SequenceNumber, aminoAcidReference));
        }
    }
}
