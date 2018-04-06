using System;
using System.Collections.Generic;

namespace ChemistryLibrary.Objects
{
    public class PeptideAnnotation : IDisposable
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

        public void Dispose()
        {
            AminoAcidReferences.ForEach(aminoAcid => aminoAcid.Dispose());
            AminoAcidReferences.Clear();
        }
    }
}