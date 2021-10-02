using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public class IntronFolder
    {
        public FoldedIntron Fold(Intron intron)
        {
            var intronLength = intron.Nucelotides.Count;
            var sequence = intron.Nucelotides;
            var reversedSequence = sequence.AsEnumerable().Reverse().ToList();
            var aligner = new SmithWatermanAligner<Nucleotide>();
            var alignment = aligner.Align(
                sequence,
                reversedSequence,
                NucleotideExtensions.IsComplementaryMatch,
                new SmithWatermanAlignerOptions
                {
                    GapPenalty = 10 // TODO: Run alignment several times, each time excluding previous match and maybe lowering gap penalty
                });
            // TODO: Apply ransac-line-fitting to detect aligning bases
            var longAlignmentMatches = alignment.Matches.Where(x => x.Length > 5).ToList();
            var basePairings = new List<BasePairing>();
            foreach (var longAlignmentMatch in longAlignmentMatches)
            {
                // TODO: Detect loop by using base1Index < base2Index at start and base1Index > base2Index at end of alignment
                for (int intraMatchBaseIndex = 0; intraMatchBaseIndex < longAlignmentMatch.Length; intraMatchBaseIndex++)
                {
                    var base1Index = intron.StartNucelotideIndex + longAlignmentMatch.Sequence1StartIndex + intraMatchBaseIndex;
                    var base2Index = intron.StartNucelotideIndex + intronLength - 1 - (longAlignmentMatch.Sequence2StartIndex + intraMatchBaseIndex);
                    var basePairing = new BasePairing(base1Index, base2Index);
                    basePairings.Add(basePairing);
                }
            }
            return new FoldedIntron(intron, basePairings);
        }

    }
}