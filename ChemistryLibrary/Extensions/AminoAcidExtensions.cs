using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Extensions
{
    public static class AminoAcidExtensions
    {
        private static readonly Dictionary<string, AminoAcidName> ThreeLetterAminoAcidMap = new Dictionary<string, AminoAcidName>
        {
            {"ILE", AminoAcidName.Isoleucine},
            {"LEU", AminoAcidName.Leucine},
            {"VAL", AminoAcidName.Valine},
            {"PHE", AminoAcidName.Phenylalanine},
            {"MET", AminoAcidName.Methionine},
            {"CYS", AminoAcidName.Cysteine},
            {"ALA", AminoAcidName.Alanine},
            {"GLY", AminoAcidName.Glycine},
            {"PRO", AminoAcidName.Proline},
            {"THR", AminoAcidName.Threonine},
            {"SER", AminoAcidName.Serine},
            {"TYR", AminoAcidName.Tyrosine},
            {"TRP", AminoAcidName.Tryptophan},
            {"GLN", AminoAcidName.Glutamine},
            {"ASN", AminoAcidName.Asparagine},
            {"HIS", AminoAcidName.Histidine},
            {"GLU", AminoAcidName.GlutamicAcid},
            {"ASP", AminoAcidName.AsparticAcid},
            {"LYS", AminoAcidName.Lysine},
            {"ARG", AminoAcidName.Arginine},
        };

        private static readonly Dictionary<AminoAcidName, string> AminoAcidThreeLetterMap =
            ThreeLetterAminoAcidMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static string ToThreeLetterCode(this AminoAcidName aminoAcidName)
        {
            return AminoAcidThreeLetterMap[aminoAcidName];
        }

        public static AminoAcidName ToAminoAcidName(this string threeLetterCode)
        {
            var upperLetterCode = threeLetterCode.ToUpperInvariant();
            if (!ThreeLetterAminoAcidMap.ContainsKey(upperLetterCode))
                throw new ChemistryException($"Unknown amino acid with code '{upperLetterCode}'"); //return AminoAcidName.Alanine;
            return ThreeLetterAminoAcidMap[upperLetterCode];
        }

        private static readonly Dictionary<char, AminoAcidName> OneLetterAminoAcidMap = new Dictionary<char, AminoAcidName>
        {
            {'I', AminoAcidName.Isoleucine},
            {'L', AminoAcidName.Leucine},
            {'V', AminoAcidName.Valine},
            {'F', AminoAcidName.Phenylalanine},
            {'M', AminoAcidName.Methionine},
            {'C', AminoAcidName.Cysteine},
            {'A', AminoAcidName.Alanine},
            {'G', AminoAcidName.Glycine},
            {'P', AminoAcidName.Proline},
            {'T', AminoAcidName.Threonine},
            {'S', AminoAcidName.Serine},
            {'Y', AminoAcidName.Tyrosine},
            {'W', AminoAcidName.Tryptophan},
            {'Q', AminoAcidName.Glutamine},
            {'N', AminoAcidName.Asparagine},
            {'H', AminoAcidName.Histidine},
            {'E', AminoAcidName.GlutamicAcid},
            {'D', AminoAcidName.AsparticAcid},
            {'K', AminoAcidName.Lysine},
            {'R', AminoAcidName.Arginine},
        };

        private static readonly Dictionary<AminoAcidName, char> AminoAcidOneLetterCodeMap =
            OneLetterAminoAcidMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static char ToOneLetterCode(this AminoAcidName aminoAcidName)
        {
            return AminoAcidOneLetterCodeMap[aminoAcidName];
        }

        public static AminoAcidName ToAminoAcidName(this char aminoAcidCode)
        {
            var upperLetterCode = char.ToUpperInvariant(aminoAcidCode);
            if(!OneLetterAminoAcidMap.ContainsKey(upperLetterCode))
                throw new ChemistryException($"Unknown amino acid with code '{upperLetterCode}'");
            return OneLetterAminoAcidMap[upperLetterCode];
        }
    }
}