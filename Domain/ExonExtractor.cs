using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain
{
    public static class ExonExtractor
    {
        public static readonly Dictionary<string, char> CordonLookup = new Dictionary<string, char>
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

        public static List<Exon> ExtractExons(string nucleotides, string peptideSequence)
        {
            peptideSequence += "#";

            char expectedAminoAcid;
            var peptideIdx = NextPeptideIndex(peptideSequence, -1, out expectedAminoAcid);
            var minimumSequenceLength = 2;
            var lastFrameWasMatch = false;
            var exons = new List<Exon>();
            Exon currentExon = null;
            for (int nucleotideIdx = 0; nucleotideIdx < nucleotides.Length;)
            {
                if (nucleotideIdx + 3 >= nucleotides.Length)
                    break;
                var frame = nucleotides.Substring(nucleotideIdx, 3);
                if (frame.Contains('N'))
                {
                    peptideIdx = NextPeptideIndex(peptideSequence, peptideIdx, out expectedAminoAcid);
                    lastFrameWasMatch = false;
                    do
                    {
                        nucleotideIdx++;
                        frame = nucleotides.Substring(nucleotideIdx, 3);
                    } while (frame.Contains('N'));
                }
                var aminoAcid = CordonLookup[frame];
                //if (lastFrameWasMatch && aminoAcid == '#' && expectedAminoAcid != '#')
                //    throw new Exception("Premature stop codon detected");

                var matchFound = aminoAcid == expectedAminoAcid;
                if (matchFound && !lastFrameWasMatch)
                {
                    for (var lookAhead = 1; lookAhead <= minimumSequenceLength; lookAhead++)
                    {
                        if (peptideIdx + lookAhead >= peptideSequence.Length)
                            break;
                        matchFound = LookAhead(nucleotides, nucleotideIdx + lookAhead * 3, peptideSequence[peptideIdx + lookAhead]);
                        if (!matchFound)
                            break;
                    }
                }
                if (!lastFrameWasMatch && currentExon != null)
                {
                    exons.Add(currentExon);
                    currentExon = null;
                }
                if (matchFound)
                {
                    if (!lastFrameWasMatch)
                    {
                        // Match last exon to currently found match, if possible
                        if (exons.Any())
                        {
                            var lastExon = exons.Last();
                            var lastExonLength = lastExon.AminoAcids.Count;
                            var previousNucleotides = nucleotides.Substring(nucleotideIdx - 3*lastExonLength,
                                3*lastExonLength);
                            var correspondingAminoAcids = AminoAcidsFromNucleotides(previousNucleotides);
                            var canAttachLastExonToCurrent = correspondingAminoAcids.SequenceEqual(lastExon.AminoAcids);
                            if (canAttachLastExonToCurrent)
                            {
                                exons.RemoveAt(exons.Count - 1);
                                currentExon = new Exon
                                {
                                    StartNucelotideIndex = nucleotideIdx - 3*lastExonLength
                                };
                                currentExon.AminoAcids.AddRange(lastExon.AminoAcids);
                            }
                        }
                        if (currentExon == null)
                            currentExon = new Exon {StartNucelotideIndex = nucleotideIdx};
                    }
                    currentExon.AminoAcids.Add(expectedAminoAcid);
                    nucleotideIdx += 3;
                    peptideIdx = NextPeptideIndex(peptideSequence, peptideIdx, out expectedAminoAcid);
                    if (peptideIdx >= peptideSequence.Length)
                        break;
                }
                else
                {
                    nucleotideIdx++;
                }
                lastFrameWasMatch = matchFound;
            }
            if (currentExon != null)
                exons.Add(currentExon);
            if (peptideIdx != peptideSequence.Length)
                throw new Exception("Sequence not fully matched");
            return exons;
        }

        private static int NextPeptideIndex(string peptideSequence, int currentIdx, out char expectedAminoAcid)
        {
            var nextIdx = currentIdx;
            do
            {
                nextIdx++;
            } while (nextIdx < peptideSequence.Length 
                && !CordonLookup.Values.Contains(peptideSequence[nextIdx]));
            expectedAminoAcid = nextIdx < peptideSequence.Length ? peptideSequence[nextIdx] : '?';
            return nextIdx;
        }

        private static IEnumerable<char> AminoAcidsFromNucleotides(string nucleotides)
        {
            if(nucleotides.Length % 3 != 0)
                throw new ArgumentException("Nucleotide sequence must be a multiple of 3");
            var aminoAcids = new List<char>();
            for (int nucleotideIdx = 0; nucleotideIdx < nucleotides.Length; nucleotideIdx += 3)
            {
                var frame = nucleotides.Substring(nucleotideIdx, 3);
                var aminoAcid = CordonLookup[frame];
                aminoAcids.Add(aminoAcid);
            }
            return aminoAcids;
        }

        private static bool LookAhead(string peptideNucleotides, int nucleotideIdx, char expectedPeptide)
        {
            if (nucleotideIdx + 3 >= peptideNucleotides.Length)
                return false;
            var nextFrame = peptideNucleotides.Substring(nucleotideIdx, 3);
            if (nextFrame.Contains('N'))
            {
                return false;
            }
            var nextAminoAcid = CordonLookup[nextFrame];
            return nextAminoAcid == expectedPeptide;
        }
    }
}