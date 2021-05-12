using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.DataLookups;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public class IntronExtronResult
    {
        public IntronExtronResult(
            List<Intron> introns,
            List<Exon> exons)
        {
            Introns = introns;
            Exons = exons;
        }

        public List<Intron> Introns { get; }
        public List<Exon> Exons { get; }
    }

    public class IntronExonExtractor
    {
        // Input data
        private string Nucleotides { get; }
        private List<AminoAcidName> AminoAcidSequence { get; }
        private readonly int minimumExonLength;

        // Output data
        private List<Intron> Introns { get; } = new List<Intron>();
        private List<Exon> Exons { get; } = new List<Exon>();


        private int nucleotideIndex;
        private int aminoAcidIndex;
        private Exon currentExon;
        private bool IsExonInProgress => currentExon != null;
        private IntronExtronResult previousResult;

        public IntronExonExtractor(
            string nucleotides,
            List<AminoAcidName> aminoAcidSequence,
            int minimumExonLength)
        {
            if (aminoAcidSequence == null || aminoAcidSequence.Count == 0)
                throw new ArgumentException("Amino acid sequence is null or empty");
            if (aminoAcidSequence.Take(aminoAcidSequence.Count - 1).Contains(AminoAcidName.StopCodon))
                throw new ArgumentException("Amino acid sequence contains stop-codon inside sequence");
            this.minimumExonLength = minimumExonLength;
            Nucleotides = nucleotides ?? throw new ArgumentNullException(nameof(nucleotides), "Nucleotide sequence is null");
            AminoAcidSequence = aminoAcidSequence;
            if(AminoAcidSequence.Last() != AminoAcidName.StopCodon)
                AminoAcidSequence.Add(AminoAcidName.StopCodon);
            if(Nucleotides.Length < 3*AminoAcidSequence.Count)
                throw new ArgumentException($"Too few nucleotides ({Nucleotides.Length}) to match {AminoAcidSequence.Count} amino acids (including stop codon)");
        }

        public IntronExonExtractor(
            string nucleotides,
            string aminoAcidSequence,
            int minimumExonLength)
            : this(nucleotides, aminoAcidSequence.Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName()).ToList(), minimumExonLength)
        {
        }

        public IntronExtronResult Extract()
        {
            if (nucleotideIndex > 0) // = Extraction has been executed before
            {
                if (previousResult != null)
                    return previousResult;
                throw new InvalidOperationException("Extraction cannot be run more than once. Previous execution failed.");
            }
            while (nucleotideIndex + 2 < Nucleotides.Length)
            {
                if (Exons.Any() && Exons.Last().AminoAcids.Count < minimumExonLength)
                {
                    RewindToStartOfLastExonPlusOneNucleotide();
                }
                var frame = Nucleotides.Substring(nucleotideIndex, 3);
                if (frame.Contains('N'))
                {
                    if (IsExonInProgress) 
                        FinishCurrentExon();
                    continue;
                }

                var aminoAcid = CodonMap.Translate(frame);
                var nextAminoAcid = AminoAcidSequence[aminoAcidIndex];
                var matchFound = aminoAcid == nextAminoAcid;
                if (matchFound)
                {
                    if (!IsExonInProgress)
                    {
                        if(CanConstructExonOfMinimumLengthAtPosition())
                            StartNewExon();
                    }

                    if (IsExonInProgress)
                    {
                        AddCodonToCurrentExon(aminoAcid, frame);
                        nucleotideIndex += 3;
                        if (aminoAcid == AminoAcidName.StopCodon)
                        {
                            FinishCurrentExon();
                            break;
                        }
                        aminoAcidIndex++;
                    }
                    else
                    {
                        nucleotideIndex++;
                    }
                }
                else
                {
                    if(IsExonInProgress)
                        FinishCurrentExon();
                    nucleotideIndex++;
                }
            }

            if (IsExonInProgress)
                FinishCurrentExon();
            if (Exons.Sum(x => x.AminoAcids.Count) != AminoAcidSequence.Count)
                throw new Exception("Sequence not fully matched");
            GenerateIntrons();
            previousResult = new IntronExtronResult(Introns, Exons);
            return previousResult;
        }

        private bool CanConstructExonOfMinimumLengthAtPosition()
        {
            if (Exons.Any())
            {
                if (CanAttachLastExonToCurrentPosition())
                    return Exons.Last().AminoAcids.Count + 1 > minimumExonLength;
            }

            var nextAminoAcids = AminoAcidSequence.SubArray(aminoAcidIndex, minimumExonLength);
            var upcomingNucleotides = Nucleotides.Substring(nucleotideIndex, 3 * minimumExonLength);
            var upcomingAminoAcids = CodonMap.AminoAcidsFromNucleotides(upcomingNucleotides);
            return upcomingAminoAcids.SequenceEqual(nextAminoAcids);
        }

        private void RewindToStartOfLastExonPlusOneNucleotide()
        {
            var lastExon = Exons.Last();
            Exons.RemoveAt(Exons.Count-1);
            nucleotideIndex = lastExon.StartNucelotideIndex + 1;
            currentExon = null;
        }

        private void GenerateIntrons()
        {
            for (var exonIndex = 0; exonIndex < Exons.Count; exonIndex++)
            {
                var exon = Exons[exonIndex];
                Intron intron;
                if (exonIndex == 0)
                {
                    if(exon.StartNucelotideIndex == 0)
                        continue;
                    intron = BuildIntron(0, exon.StartNucelotideIndex - 1);
                    Introns.Add(intron);
                }
                if (exonIndex == Exons.Count - 1)
                {
                    // Last exon
                    if(exon.EndNucleotideIndex == Nucleotides.Length - 1)
                        continue;
                    intron = BuildIntron(exon.EndNucleotideIndex + 1, Nucleotides.Length - 1);
                    Introns.Add(intron);
                }
                else
                {
                    var nextExon = Exons[exonIndex + 1];
                    intron = BuildIntron(exon.EndNucleotideIndex + 1, nextExon.StartNucelotideIndex - 1);
                    Introns.Add(intron);
                }
            }
        }

        private Intron BuildIntron(
            int startIndex,
            int endIndex)
        {
            return new Intron(startIndex, ToNucleotides(Nucleotides.Substring(startIndex, endIndex - startIndex + 1)));
        }

        private void AddCodonToCurrentExon(
            AminoAcidName aminoAcid,
            string frame)
        {
            var codon = new Codon(aminoAcid, ToNucleotides(frame));
            currentExon.AminoAcids.Add(codon);
        }

        private void StartNewExon()
        {
            var attachedCodons = new List<Codon>();
            while (Exons.Any() && CanAttachLastExonToCurrentPosition())
            {
                var lastExon = Exons.Last();
                Exons.RemoveAt(Exons.Count - 1);
                attachedCodons.AddRange(lastExon.AminoAcids);
            }

            currentExon = new Exon(nucleotideIndex - 3 * attachedCodons.Count, attachedCodons);
        }

        private bool CanAttachLastExonToCurrentPosition()
        {
            var lastExon = Exons.Last();
            var lastExonLength = lastExon.AminoAcids.Count;
            var previousNucleotides = Nucleotides.Substring(nucleotideIndex - 3 * lastExonLength, 3 * lastExonLength);
            var correspondingAminoAcids = CodonMap.AminoAcidsFromNucleotides(previousNucleotides);
            var canAttachLastExonToCurrent = correspondingAminoAcids.SequenceEqual(lastExon.AminoAcids.Select(x => x.AminoAcid));
            return canAttachLastExonToCurrent;
        }

        private void FinishCurrentExon()
        {
            Exons.Add(currentExon);
            currentExon = null;
        }

        private static List<Nucleotide> ToNucleotides(string frame)
        {
            return frame.Select(nucleotideCode => (Nucleotide) Enum.Parse(typeof(Nucleotide), nucleotideCode + "")).ToList();
        }
    }
}