using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Builders
{
    public static class PeptideBuilder
    {
        public static Peptide PeptideFromString(string peptideString)
        {
            var sequence = Regex.Replace(peptideString.ToUpperInvariant(), "[^A-Z]", "")
                .Select(aminoAcidCode => AminoAcidExtensions.ToAminoAcidName((char) aminoAcidCode));
            return PeptideFromSequence(sequence);
        }

        public static Peptide PeptideFromSequence(IEnumerable<AminoAcidName> cleanedPeptideString)
        {
            var aminoAcidReferences = cleanedPeptideString.Select(AminoAcidLibrary.Get);
            return PeptideFromAminoAcids(aminoAcidReferences);
        }

        public static Peptide PeptideFromAminoAcids(IEnumerable<AminoAcidReference> aminoAcidReferences)
        {
            var aminoAcids = new List<AminoAcidReference>();
            MoleculeReference moleculeReference = null;
            foreach (var aminoAcid in aminoAcidReferences)
            {
                MoleculeReference aminoAcidReference;
                if (moleculeReference != null)
                {
                    moleculeReference = moleculeReference.Add(aminoAcid, out aminoAcidReference);
                }
                else
                {
                    moleculeReference = aminoAcid;
                    aminoAcidReference = new MoleculeReference(
                        aminoAcid.Molecule,
                        aminoAcid.VertexIds,
                        aminoAcid.FirstAtomId,
                        aminoAcid.LastAtomId);
                }
                aminoAcids.Add(new AminoAcidReference(aminoAcid.Name, aminoAcidReference));
            }
            return new Peptide(moleculeReference, aminoAcids);
        }
    }
}
