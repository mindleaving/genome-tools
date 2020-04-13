using System;
using System.Collections.Generic;

namespace ChemistryLibrary.Objects
{
    public class PeptideAnnotation<T> : IDisposable
    {
        public PeptideAnnotation(
            PeptideSecondaryStructure type,
            List<T> aminoAcidReferences)
        {
            Type = type;
            AminoAcidReferences = aminoAcidReferences;
        }

        public PeptideSecondaryStructure Type { get; }
        public List<T> AminoAcidReferences { get; }

        public void Dispose()
        {
            AminoAcidReferences.ForEach(aminoAcid => (aminoAcid as IDisposable)?.Dispose());
            AminoAcidReferences.Clear();
        }
    }
}