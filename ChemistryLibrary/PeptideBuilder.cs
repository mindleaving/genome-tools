using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChemistryLibrary
{
    public static class PeptideBuilder
    {
        public static Peptide PeptideFromString(string peptideString)
        {
            var sequence = Regex.Replace(peptideString.ToUpperInvariant(), "[^A-Z]", "")
                .Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName());
            return PeptideFromSequence(sequence);
        }

        public static Peptide PeptideFromSequence(IEnumerable<AminoAcidName> cleanedPeptideString)
        {
            var aminoAcids = new List<AminoAcidReference>();
            MoleculeReference moleculeReference = null;
            foreach (var aminoAcidCode in cleanedPeptideString)
            {
                var aminoAcid = MapLetterToAminoAcid(aminoAcidCode);
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

        private static AminoAcidReference MapLetterToAminoAcid(AminoAcidName aminoAcidCode)
        {
            switch (aminoAcidCode)
            {
                case AminoAcidName.Isoleucine:
                    return AminoAcidLibrary.Isoleucine;
                case AminoAcidName.Leucine:
                    return AminoAcidLibrary.Leucine;
                case AminoAcidName.Valine:
                    return AminoAcidLibrary.Valine;
                case AminoAcidName.Phenylalanine:
                    return AminoAcidLibrary.Phenylalanine;
                case AminoAcidName.Methionine:
                    return AminoAcidLibrary.Methionine;
                case AminoAcidName.Cysteine:
                    return AminoAcidLibrary.Cysteine;
                case AminoAcidName.Alanine:
                    return AminoAcidLibrary.Alanine;
                case AminoAcidName.Glycine:
                    return AminoAcidLibrary.Glycine;
                case AminoAcidName.Proline:
                    return AminoAcidLibrary.Proline;
                case AminoAcidName.Threonine:
                    return AminoAcidLibrary.Threonine;
                case AminoAcidName.Serine:
                    return AminoAcidLibrary.Serine;
                case AminoAcidName.Tyrosine:
                    return AminoAcidLibrary.Tyrosine;
                case AminoAcidName.Tryptophan:
                    return AminoAcidLibrary.Tryptophan;
                case AminoAcidName.Glutamine:
                    return AminoAcidLibrary.Glutamine;
                case AminoAcidName.Asparagine:
                    return AminoAcidLibrary.Asparagine;
                case AminoAcidName.Histidine:
                    return AminoAcidLibrary.Histidine;
                case AminoAcidName.GlutamicAcid:
                    return AminoAcidLibrary.GlutamicAcid;
                case AminoAcidName.AsparticAcid:
                    return AminoAcidLibrary.AsparticAcid;
                case AminoAcidName.Lysine:
                    return AminoAcidLibrary.Lysine;
                case AminoAcidName.Arginine:
                    return AminoAcidLibrary.Arginine;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aminoAcidCode), $"Unknown amino acid code '{aminoAcidCode}'");
            }
        }
    }
}
