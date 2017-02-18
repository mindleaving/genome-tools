using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChemistryLibrary
{
    public static class PeptideBuilder
    {
        public static Peptide PeptideFromString(string peptideString)
        {
            var cleanedPeptideString = Regex.Replace(
                peptideString.ToUpperInvariant(),
                "[^A-Z]", "");
            var aminoAcids = new List<AminoAcidReference>();
            MoleculeReference moleculeReference = null;
            foreach (var aminoAcidCode in cleanedPeptideString)
            {
                if(aminoAcidCode == '#')
                    break;
                var aminoAcid = MapLetterToAminoAcid(aminoAcidCode);
                MoleculeReference aminoAcidReference;
                if (moleculeReference != null)
                {
                    moleculeReference = moleculeReference.Add(aminoAcid, out aminoAcidReference);
                }
                else
                {
                    moleculeReference = aminoAcid;
                    aminoAcidReference = new MoleculeReference(aminoAcid.Molecule, aminoAcid.VertexIds);
                }
                aminoAcids.Add(new AminoAcidReference(aminoAcid.Name, aminoAcidReference));
            }
            return new Peptide(moleculeReference, aminoAcids);
        }

        private static AminoAcidReference MapLetterToAminoAcid(char aminoAcidCode)
        {
            switch (aminoAcidCode)
            {
                case 'I':
                    return AminoAcidLibrary.Isoleucine;
                case 'L':
                    return AminoAcidLibrary.Leucine;
                case 'V':
                    return AminoAcidLibrary.Valine;
                case 'F':
                    return AminoAcidLibrary.Phenylalanine;
                case 'M':
                    return AminoAcidLibrary.Methionine;
                case 'C':
                    return AminoAcidLibrary.Cysteine;
                case 'A':
                    return AminoAcidLibrary.Alanine;
                case 'G':
                    return AminoAcidLibrary.Glycine;
                case 'P':
                    return AminoAcidLibrary.Proline;
                case 'T':
                    return AminoAcidLibrary.Threonine;
                case 'S':
                    return AminoAcidLibrary.Serine;
                case 'Y':
                    return AminoAcidLibrary.Tyrosine;
                case 'W':
                    return AminoAcidLibrary.Tryptophan;
                case 'Q':
                    return AminoAcidLibrary.Glutamine;
                case 'N':
                    return AminoAcidLibrary.Asparagine;
                case 'H':
                    return AminoAcidLibrary.Histidine;
                case 'E':
                    return AminoAcidLibrary.GlutamicAcid;
                case 'D':
                    return AminoAcidLibrary.AsparticAcid;
                case 'K':
                    return AminoAcidLibrary.Lysine;
                case 'R':
                    return AminoAcidLibrary.Arginine;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aminoAcidCode), $"Unknown amino acid code '{aminoAcidCode}'");
            }
        }
    }
}
