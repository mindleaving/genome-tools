using System;
using System.Text.RegularExpressions;

namespace ChemistryLibrary
{
    public static class PeptideBuilder
    {
        public static MoleculeReference PeptideFromString(string peptideString)
        {
            var cleanedPeptideString = Regex.Replace(
                peptideString.ToUpperInvariant(),
                "[^A-Z]", "");
            MoleculeReference moleculeReference = null;
            foreach (var aminoAcidCode in cleanedPeptideString)
            {
                if(aminoAcidCode == '#')
                    break;
                var aminoAcid = MapLetterToAminoAcid(aminoAcidCode);
                moleculeReference = moleculeReference != null ? moleculeReference.Add(aminoAcid) : aminoAcid;
            }
            return moleculeReference;
        }

        private static MoleculeReference MapLetterToAminoAcid(char aminoAcidCode)
        {
            switch (aminoAcidCode)
            {
                case 'I':
                    return AminoAcidLibrary.IsoLeucine;
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
