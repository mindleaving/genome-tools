using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.DataLookups
{
    public static class CodonMap
    {
        private static readonly Dictionary<string, AminoAcidName> CodonLookup = new Dictionary<string, AminoAcidName>
        {
            {"AAA", AminoAcidName.Lysine},
            {"TAA", AminoAcidName.StopCodon},
            {"GAA", AminoAcidName.GlutamicAcid},
            {"CAA", AminoAcidName.Glutamine},
            {"ATA", AminoAcidName.Isoleucine},
            {"TTA", AminoAcidName.Leucine},
            {"GTA", AminoAcidName.Valine},
            {"CTA", AminoAcidName.Leucine},
            {"AGA", AminoAcidName.Arginine},
            {"TGA", AminoAcidName.StopCodon},
            {"GGA", AminoAcidName.Glycine},
            {"CGA", AminoAcidName.Arginine},
            {"ACA", AminoAcidName.Threonine},
            {"TCA", AminoAcidName.Serine},
            {"GCA", AminoAcidName.Alanine},
            {"CCA", AminoAcidName.Proline},
            {"AAT", AminoAcidName.Asparagine},
            {"TAT", AminoAcidName.Tyrosine},
            {"GAT", AminoAcidName.AsparticAcid},
            {"CAT", AminoAcidName.Histidine},
            {"ATT", AminoAcidName.Isoleucine},
            {"TTT", AminoAcidName.Phenylalanine},
            {"GTT", AminoAcidName.Valine},
            {"CTT", AminoAcidName.Leucine},
            {"AGT", AminoAcidName.Serine},
            {"TGT", AminoAcidName.Cysteine},
            {"GGT", AminoAcidName.Glycine},
            {"CGT", AminoAcidName.Arginine},
            {"ACT", AminoAcidName.Threonine},
            {"TCT", AminoAcidName.Serine},
            {"GCT", AminoAcidName.Alanine},
            {"CCT", AminoAcidName.Proline},
            {"AAG", AminoAcidName.Lysine},
            {"TAG", AminoAcidName.StopCodon},
            {"GAG", AminoAcidName.GlutamicAcid},
            {"CAG", AminoAcidName.Glutamine},
            {"ATG", AminoAcidName.Methionine},
            {"TTG", AminoAcidName.Leucine},
            {"GTG", AminoAcidName.Valine},
            {"CTG", AminoAcidName.Leucine},
            {"AGG", AminoAcidName.Arginine},
            {"TGG", AminoAcidName.Tryptophan},
            {"GGG", AminoAcidName.Glycine},
            {"CGG", AminoAcidName.Arginine},
            {"ACG", AminoAcidName.Threonine},
            {"TCG", AminoAcidName.Serine},
            {"GCG", AminoAcidName.Alanine},
            {"CCG", AminoAcidName.Proline},
            {"AAC", AminoAcidName.Asparagine},
            {"TAC", AminoAcidName.Tyrosine},
            {"GAC", AminoAcidName.AsparticAcid},
            {"CAC", AminoAcidName.Histidine},
            {"ATC", AminoAcidName.Isoleucine},
            {"TTC", AminoAcidName.Phenylalanine},
            {"GTC", AminoAcidName.Valine},
            {"CTC", AminoAcidName.Leucine},
            {"AGC", AminoAcidName.Serine},
            {"TGC", AminoAcidName.Cysteine},
            {"GGC", AminoAcidName.Glycine},
            {"CGC", AminoAcidName.Arginine},
            {"ACC", AminoAcidName.Threonine},
            {"TCC", AminoAcidName.Serine},
            {"GCC", AminoAcidName.Alanine},
            {"CCC", AminoAcidName.Proline}
        };

        public static AminoAcidName Translate(string codon)
        {
            return CodonLookup[codon];
        }

        public static IEnumerable<AminoAcidName> AminoAcidsFromNucleotides(string nucleotides)
        {
            if (nucleotides.Length % 3 != 0)
                throw new ArgumentException("Nucleotide sequence must be a multiple of 3");
            var aminoAcids = new List<AminoAcidName>();
            for (int nucleotideIdx = 0; nucleotideIdx < nucleotides.Length; nucleotideIdx += 3)
            {
                var frame = nucleotides.Substring(nucleotideIdx, 3);
                var aminoAcid = Translate(frame);
                aminoAcids.Add(aminoAcid);
            }
            return aminoAcids;
        }

        public static string GenerateCodonForAminoAcid(AminoAcidName aminoAcidName)
        {
            var matchingCodons = CodonLookup
                .Where(x => x.Value == aminoAcidName)
                .Select(kvp => kvp.Key)
                .ToList();
            if (matchingCodons.Count == 0)
                throw new Exception($"No codon exists for amino acid '{aminoAcidName}'");
            var codonIndex = StaticRandom.Rng.Next(matchingCodons.Count);
            return matchingCodons[codonIndex];
        }
    }
}