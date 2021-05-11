using System;
using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.DataLookups;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Measurements
{
    public static class IntronExonExtractor
    {
        public static IntronExtronResult ExtractExons(
            string nucleotides,
            string peptideSequence)
        {
            return ExtractExons(nucleotides, peptideSequence.Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName()).ToList());
        }

        public static IntronExtronResult ExtractExons(string nucleotides, List<AminoAcidName> peptideSequence)
        {
            if (peptideSequence.Last() != AminoAcidName.StopCodon)
                peptideSequence = peptideSequence.Concat(new[] {AminoAcidName.StopCodon}).ToList();

            var peptideIdx = NextPeptideIndex(peptideSequence, -1, out var expectedAminoAcid);
            var minimumSequenceLength = 2;
            var lastFrameWasMatch = false;
            var exons = new List<Exon>();
            var introns = new List<Intron>();
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
                var aminoAcid = CodonMap.Translate(frame);
                //if (lastFrameWasMatch && aminoAcid == '#' && expectedAminoAcid != '#')
                //    throw new Exception("Premature stop codon detected");

                var matchFound = aminoAcid == expectedAminoAcid;
                if (matchFound && !lastFrameWasMatch)
                {
                    for (var lookAhead = 1; lookAhead <= minimumSequenceLength; lookAhead++)
                    {
                        if (peptideIdx + lookAhead >= peptideSequence.Count)
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
                            var correspondingAminoAcids = CodonMap.AminoAcidsFromNucleotides(previousNucleotides);
                            var canAttachLastExonToCurrent = correspondingAminoAcids.SequenceEqual(lastExon.AminoAcids.Select(x => x.AminoAcid));
                            if (canAttachLastExonToCurrent)
                            {
                                exons.RemoveAt(exons.Count - 1);
                                currentExon = new Exon(nucleotideIdx - 3*lastExonLength, lastExon.AminoAcids);
                            }
                        }
                        if (currentExon == null)
                            currentExon = new Exon(nucleotideIdx, new List<Codon>());
                        var intronStart = exons.Any() ? exons.Last().EndNucleotideIndex + 1 : 0;
                        var intronEnd = currentExon.StartNucelotideIndex - 1;
                        var intronNucleotides = nucleotides.Substring(intronStart, intronEnd - intronStart + 1)
                            .Select(nucleotideCode => (Nucleotide)Enum.Parse(typeof(Nucleotide), nucleotideCode + ""))
                            .ToList();
                        throw new NotImplementedException("Intron creation is not correct yet");
                        introns.Add(new Intron(intronStart, intronNucleotides));
                    }

                    var codon = new Codon(aminoAcid, ToNucleotides(frame));
                    currentExon.AminoAcids.Add(codon);
                    nucleotideIdx += 3;
                    peptideIdx = NextPeptideIndex(peptideSequence, peptideIdx, out expectedAminoAcid);
                    if (peptideIdx >= peptideSequence.Count)
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
            if (peptideIdx != peptideSequence.Count)
                throw new Exception("Sequence not fully matched");
            return new IntronExtronResult(introns, exons);
        }

        private static List<Nucleotide> ToNucleotides(string frame)
        {
            return frame.Select(nucleotideCode => (Nucleotide)Enum.Parse(typeof(Nucleotide), nucleotideCode + "")).ToList();
        }

        private static int NextPeptideIndex(List<AminoAcidName> peptideSequence, int currentIdx, out AminoAcidName expectedAminoAcid)
        {
            var nextIdx = currentIdx + 1;
            expectedAminoAcid = nextIdx < peptideSequence.Count ? peptideSequence[nextIdx] : AminoAcidName.Unknown;
            return nextIdx;
        }

        private static bool LookAhead(string peptideNucleotides, int nucleotideIdx, AminoAcidName expectedAminoAcid)
        {
            if (nucleotideIdx + 3 >= peptideNucleotides.Length)
                return false;
            var nextFrame = peptideNucleotides.Substring(nucleotideIdx, 3);
            if (nextFrame.Contains('N'))
            {
                return false;
            }
            var nextAminoAcid = CodonMap.Translate(nextFrame);
            return nextAminoAcid == expectedAminoAcid;
        }
    }
}