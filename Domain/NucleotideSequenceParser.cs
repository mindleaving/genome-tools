using System;
using System.Collections.Generic;

namespace Domain
{
    public static class NucleotideSequenceParser
    {
        public static readonly Dictionary<string, char> CodonLookup = new Dictionary<string, char>
        {
            {"AAA", 'K'},
            {"TAA", '#'},
            {"GAA", 'E'},
            {"CAA", 'Q'},
            {"ATA", 'I'},
            {"TTA", 'L'},
            {"GTA", 'V'},
            {"CTA", 'L'},
            {"AGA", 'R'},
            {"TGA", '#'},
            {"GGA", 'G'},
            {"CGA", 'R'},
            {"ACA", 'T'},
            {"TCA", 'S'},
            {"GCA", 'A'},
            {"CCA", 'P'},
            {"AAT", 'N'},
            {"TAT", 'Y'},
            {"GAT", 'D'},
            {"CAT", 'H'},
            {"ATT", 'I'},
            {"TTT", 'F'},
            {"GTT", 'V'},
            {"CTT", 'L'},
            {"AGT", 'S'},
            {"TGT", 'C'},
            {"GGT", 'G'},
            {"CGT", 'R'},
            {"ACT", 'T'},
            {"TCT", 'S'},
            {"GCT", 'A'},
            {"CCT", 'P'},
            {"AAG", 'K'},
            {"TAG", '#'},
            {"GAG", 'E'},
            {"CAG", 'Q'},
            {"ATG", 'M'},
            {"TTG", 'L'},
            {"GTG", 'V'},
            {"CTG", 'L'},
            {"AGG", 'R'},
            {"TGG", 'W'},
            {"GGG", 'G'},
            {"CGG", 'R'},
            {"ACG", 'T'},
            {"TCG", 'S'},
            {"GCG", 'A'},
            {"CCG", 'P'},
            {"AAC", 'N'},
            {"TAC", 'Y'},
            {"GAC", 'D'},
            {"CAC", 'H'},
            {"ATC", 'I'},
            {"TTC", 'F'},
            {"GTC", 'V'},
            {"CTC", 'L'},
            {"AGC", 'S'},
            {"TGC", 'C'},
            {"GGC", 'G'},
            {"CGC", 'R'},
            {"ACC", 'T'},
            {"TCC", 'S'},
            {"GCC", 'A'},
            {"CCC", 'P'}
        };

        public static IEnumerable<char> AminoAcidsFromNucleotides(string nucleotides)
        {
            if (nucleotides.Length % 3 != 0)
                throw new ArgumentException("Nucleotide sequence must be a multiple of 3");
            var aminoAcids = new List<char>();
            for (int nucleotideIdx = 0; nucleotideIdx < nucleotides.Length; nucleotideIdx += 3)
            {
                var frame = nucleotides.Substring(nucleotideIdx, 3);
                var aminoAcid = CodonLookup[frame];
                aminoAcids.Add(aminoAcid);
            }
            return aminoAcids;
        }
    }
}