using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain
{
    public static class ExonExtractor
    {
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
                var aminoAcid = NucleotideSequenceParser.CodonLookup[frame];
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
                            var correspondingAminoAcids = NucleotideSequenceParser.AminoAcidsFromNucleotides(previousNucleotides);
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
                && !NucleotideSequenceParser.CodonLookup.Values.Contains(peptideSequence[nextIdx]));
            expectedAminoAcid = nextIdx < peptideSequence.Length ? peptideSequence[nextIdx] : '?';
            return nextIdx;
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
            var nextAminoAcid = NucleotideSequenceParser.CodonLookup[nextFrame];
            return nextAminoAcid == expectedPeptide;
        }
    }
}