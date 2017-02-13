using System;

namespace ChemistryLibrary
{
    public enum AminoAcidName
    {
        Alanine,
        Glycine,
        Isoleucine,
        Leucine,
        Proline,
        Valine,
        Phenylalanine,
        Tryptophan,
        Tyrosine,
        AsparticAcid,
        GlutamicAcid,
        Arginine,
        Histidine,
        Lysine,
        Serine,
        Threonine,
        Cysteine,
        Methionine,
        Asparagine,
        Glutamine
    }

    public static class AminoAcidExtensions
    {
        public static string ToThreeLetterCode(this AminoAcidName aminoAcidName)
        {
            switch (aminoAcidName)
            {
                case AminoAcidName.Alanine:
                    return "ALA";
                case AminoAcidName.Glycine:
                    return "GLY";
                case AminoAcidName.Isoleucine:
                    return "ILE";
                case AminoAcidName.Leucine:
                    return "LEU";
                case AminoAcidName.Proline:
                    return "PRO";
                case AminoAcidName.Valine:
                    return "VAL";
                case AminoAcidName.Phenylalanine:
                    return "PHE";
                case AminoAcidName.Tryptophan:
                    return "TRP";
                case AminoAcidName.Tyrosine:
                    return "TYR";
                case AminoAcidName.AsparticAcid:
                    return "ASP";
                case AminoAcidName.GlutamicAcid:
                    return "GLU";
                case AminoAcidName.Arginine:
                    return "ARG";
                case AminoAcidName.Histidine:
                    return "HIS";
                case AminoAcidName.Lysine:
                    return "LYS";
                case AminoAcidName.Serine:
                    return "SER";
                case AminoAcidName.Threonine:
                    return "THR";
                case AminoAcidName.Cysteine:
                    return "CYS";
                case AminoAcidName.Methionine:
                    return "MET";
                case AminoAcidName.Asparagine:
                    return "ASN";
                case AminoAcidName.Glutamine:
                    return "GLN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(aminoAcidName), aminoAcidName, null);
            }
        }
    }
}