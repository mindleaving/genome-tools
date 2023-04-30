using System;
using System.Collections.Generic;
using System.Linq;
using Commons;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.Measurements;

namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public class LongSequenceAligner
    {
        private readonly LongSequenceAlignerOptions options;
        private readonly SmithWatermanAligner<char> smithWatermanAligner = new();

        public LongSequenceAligner(LongSequenceAlignerOptions options = null)
        {
            this.options = options ?? new LongSequenceAlignerOptions();
        }

        public List<IAlignmentRegion> Align(string referenceSequence, string sequenceToBeAligned)
        {
            var referenceSequenceProxy = new StringProxy(referenceSequence, 0, referenceSequence.Length - 1);
            var sequenceToBeAlignedProxy = new StringProxy(sequenceToBeAligned, 0, sequenceToBeAligned.Length - 1);
            var unalignedParts = new Queue<UnalignedPart>(new[] { new UnalignedPart(sequenceToBeAlignedProxy, referenceSequenceProxy) });
            var alignedRegions = new List<MatchedAlignmentRegion>();
            var unmatchedRegions = new List<UnalignedPart>();
            while (unalignedParts.Any())
            {
                var unalignedPart = unalignedParts.Dequeue();
                //if (unalignedPart.SequenceToBeAligned.Length <= options.ShortSequenceThreshold 
                //    && unalignedPart.ReferenceSequence.Length <= options.ShortSequenceThreshold)
                //{
                //    var matchedRegions = AlignShortRegion(unalignedPart);
                //    alignedRegions.AddRange(matchedRegions);
                //    continue;
                //}

                var matchedRegion = AlignLongRegion(unalignedPart);
                if(matchedRegion == null)
                {
                    unmatchedRegions.Add(unalignedPart);
                    continue;
                }
                alignedRegions.Add(matchedRegion);
                var newUnalignedParts = CreateUnalignedParts(unalignedPart, matchedRegion);
                newUnalignedParts.ForEach(unalignedParts.Enqueue);
            }
            alignedRegions.Sort((a,b) => a.ReferenceStartIndex.CompareTo(b.ReferenceStartIndex));

            var diffRegions = GenerateDifferenceRegions(alignedRegions, sequenceToBeAlignedProxy.Length);

            return alignedRegions
                .Concat(diffRegions)
                .OrderBy(x => x.ReferenceStartIndex)
                .ThenBy(x => x.Type == AlignmentRegionType.Insert ? 1 : 2)
                .ToList();
        }

        private List<IAlignmentRegion> GenerateDifferenceRegions(
            List<MatchedAlignmentRegion> alignedRegions,
            int sequenceToBeAlignedLength)
        {
            var diffRegions = new List<IAlignmentRegion>();
            for (int alignedRegionIndex = 0; alignedRegionIndex+1 < alignedRegions.Count; alignedRegionIndex++)
            {
                var alignedRegion1 = alignedRegions[alignedRegionIndex];
                var alignedRegion2 = alignedRegions[alignedRegionIndex + 1];
                //if (alignedRegionIndex == 0)
                //{
                //    if (alignedRegion1.AlignedSequenceStartIndex > 0)
                //    {
                //        var misMatchedRegion = new MismatchAlignmentRegion(
                //            alignedRegion1.ReferenceStartIndex-alignedRegion1.AlignedSequenceStartIndex, 
                //            alignedRegion1.ReferenceStartIndex-1,
                //            0,
                //            alignedRegion1.AlignedSequenceStartIndex-1);
                //        diffRegions.Add(misMatchedRegion);
                //    }
                //}

                //if (alignedRegionIndex == alignedRegions.Count - 2)
                //{
                //    if (alignedRegion2.AlignedSequenceEndIndex < sequenceToBeAlignedLength - 1)
                //    {
                //        var misMatchedRegion = new MismatchAlignmentRegion(
                //            alignedRegion2.ReferenceEndIndex + 1,
                //            alignedRegion2.ReferenceEndIndex + sequenceToBeAlignedLength-1 - alignedRegion2.AlignedSequenceEndIndex,
                //            alignedRegion2.AlignedSequenceEndIndex + 1,
                //            sequenceToBeAlignedLength-1);
                //        diffRegions.Add(misMatchedRegion);
                //    }
                //}

                if (alignedRegion2.ReferenceStartIndex - alignedRegion1.ReferenceEndIndex == 1)
                {
                    var insertion = new InsertAlignmentRegion(
                        alignedRegion2.ReferenceStartIndex,
                        alignedRegion1.AlignedSequenceEndIndex + 1,
                        alignedRegion2.AlignedSequenceStartIndex - alignedRegion1.AlignedSequenceEndIndex - 1);
                    diffRegions.Add(insertion);
                }
                else if (alignedRegion2.AlignedSequenceStartIndex - alignedRegion1.AlignedSequenceEndIndex == 1)
                {
                    var deletion = new DeletionAlignmentRegion(
                        alignedRegion1.ReferenceEndIndex + 1,
                        alignedRegion2.ReferenceStartIndex - alignedRegion1.ReferenceEndIndex - 1);
                    diffRegions.Add(deletion);
                }
                else
                {
                    var misMatchedRegion = new MismatchAlignmentRegion(
                        alignedRegion1.ReferenceEndIndex+1,
                        alignedRegion2.ReferenceStartIndex-1,
                        alignedRegion1.AlignedSequenceEndIndex+1,
                        alignedRegion2.AlignedSequenceStartIndex-1);
                    diffRegions.Add(misMatchedRegion);
                }
            }
            return diffRegions;
        }

        private MatchedAlignmentRegion AlignLongRegion(UnalignedPart unalignedPart)
        {
            var seed = FindSeed(unalignedPart);
            if (seed == null)
                return null;
            var matchedRegion = ExpandSeed(unalignedPart, seed);
            return matchedRegion;
        }

        private IEnumerable<UnalignedPart> CreateUnalignedParts(
            UnalignedPart unalignedPart, 
            MatchedAlignmentRegion matchedRegion)
        {
            if (matchedRegion.AlignedSequenceStartIndex > unalignedPart.SequenceToBeAligned.StartIndex
                && matchedRegion.ReferenceStartIndex > unalignedPart.ReferenceSequence.StartIndex)
            {
                var frontUnalignedSequence = unalignedPart.SequenceToBeAligned.Substring(0, matchedRegion.AlignedSequenceStartIndex-unalignedPart.SequenceToBeAligned.StartIndex);
                var referenceSequence = unalignedPart.ReferenceSequence.Substring(0, matchedRegion.ReferenceStartIndex-unalignedPart.ReferenceSequence.StartIndex);
                var frontAlignmentPart = new UnalignedPart(
                    frontUnalignedSequence, 
                    referenceSequence);
                yield return frontAlignmentPart;
            }

            if (matchedRegion.AlignedSequenceEndIndex < unalignedPart.SequenceToBeAligned.EndIndex
                && matchedRegion.ReferenceEndIndex < unalignedPart.ReferenceSequence.EndIndex)
            {
                var trailingUnalignedSequence = unalignedPart.SequenceToBeAligned.Substring(matchedRegion.AlignedSequenceEndIndex-unalignedPart.SequenceToBeAligned.StartIndex + 1);
                var referenceSequence = unalignedPart.ReferenceSequence.Substring(matchedRegion.ReferenceEndIndex-unalignedPart.ReferenceSequence.StartIndex + 1);
                var trailingAlignmentPart = new UnalignedPart(
                    trailingUnalignedSequence, 
                    referenceSequence);
                yield return trailingAlignmentPart;
            }
        }

        private List<MatchedAlignmentRegion> AlignShortRegion(UnalignedPart unalignedPart)
        {
            try
            {
                var smithWatermanAlignmentResult = smithWatermanAligner.Align(
                    unalignedPart.ReferenceSequence.ToList(),
                    unalignedPart.SequenceToBeAligned.ToList(),
                    (a, b) => a == b);
                var matchedRegions = new List<MatchedAlignmentRegion>();
                foreach (var alignmentMatch in smithWatermanAlignmentResult.Matches)
                {
                    var matchedRegion = new MatchedAlignmentRegion(
                        unalignedPart.ReferenceSequence.StartIndex + alignmentMatch.Sequence1StartIndex, 
                        unalignedPart.SequenceToBeAligned.StartIndex + alignmentMatch.Sequence2StartIndex, 
                        alignmentMatch.Length);
                    matchedRegions.Add(matchedRegion);
                }
                return matchedRegions;
            }
            catch
            {
                return new List<MatchedAlignmentRegion>();
            }
        }

        private MatchedAlignmentRegion ExpandSeed(UnalignedPart unalignedPart, AlignmentSeed seed)
        {
            var referenceStartIndex = seed.ReferenceStartIndex;
            var sequenceToBeAlignedStartIndex = seed.SequenceToBeAlignedStartIndex;
            while (referenceStartIndex > 0
                   && sequenceToBeAlignedStartIndex > 0 
                   && unalignedPart.ReferenceSequence[referenceStartIndex-1] == unalignedPart.SequenceToBeAligned[sequenceToBeAlignedStartIndex-1])
            {
                referenceStartIndex--;
                sequenceToBeAlignedStartIndex--;
            }

            var referenceEndIndex = seed.ReferenceStartIndex + seed.SeedLength - 1;
            var sequenceToBeAlignedEndIndex = seed.SequenceToBeAlignedStartIndex + seed.SeedLength - 1;
            while (referenceEndIndex+1 < unalignedPart.ReferenceSequence.Length
                   && sequenceToBeAlignedEndIndex+1 < unalignedPart.SequenceToBeAligned.Length
                   && unalignedPart.ReferenceSequence[referenceEndIndex+1] == unalignedPart.SequenceToBeAligned[sequenceToBeAlignedEndIndex+1])
            {
                referenceEndIndex++;
                sequenceToBeAlignedEndIndex++;
            }
            return new MatchedAlignmentRegion(
                unalignedPart.ReferenceSequence.StartIndex + referenceStartIndex, 
                unalignedPart.SequenceToBeAligned.StartIndex + sequenceToBeAlignedStartIndex, 
                referenceEndIndex - referenceStartIndex + 1);
        }

        private AlignmentSeed FindSeed(UnalignedPart unalignedPart)
        {
            var shorterSequence = unalignedPart.ReferenceSequence.Length < unalignedPart.SequenceToBeAligned.Length
                ? SequenceType.Reference
                : SequenceType.ToBeAligned;
            var shortSequence = shorterSequence == SequenceType.Reference ? unalignedPart.ReferenceSequence : unalignedPart.SequenceToBeAligned;
            var longSequence = shorterSequence == SequenceType.Reference ? unalignedPart.SequenceToBeAligned : unalignedPart.ReferenceSequence;
            var maxSeedLength = shortSequence.Length;
            var seedLength = Math.Min(maxSeedLength, options.DefaultSeedLength);

            var seedLengthHistory = new List<int>();
            var maximumTriesPerSeedLength = Math.Min(shortSequence.Length-seedLength+1, options.MaximumSeedingTries);
            while (seedLength > 0 && seedLength <= maxSeedLength && !seedLengthHistory.Contains(seedLength))
            {
                seedLengthHistory.Add(seedLength);
                var lastValidSeedPosition = shortSequence.Length - seedLength;
                var noMatchCount = 0;
                var multipleMatchesCount = 0;
                var tryCount = 0;
                while (tryCount < maximumTriesPerSeedLength)
                {
                    var seedPositionInShortSequence = StaticRandom.Rng.Next(lastValidSeedPosition + 1);
                    var seedCandidate = shortSequence.Substring(seedPositionInShortSequence, seedLength);
                    var seedPositionInLongSequence = longSequence.IndexOf(seedCandidate);
                    if (seedPositionInLongSequence >= 0)
                    {
                        var hasMoreMatches = seedPositionInLongSequence < longSequence.Length - 1
                            && longSequence.Substring(seedPositionInLongSequence + 1).IndexOf(seedCandidate) >= 0;
                        if (!hasMoreMatches)
                        {
                            return new AlignmentSeed(
                                shorterSequence == SequenceType.ToBeAligned ? seedPositionInShortSequence : seedPositionInLongSequence, 
                                shorterSequence == SequenceType.ToBeAligned ? seedPositionInLongSequence : seedPositionInShortSequence, 
                                seedLength);
                        }
                        multipleMatchesCount++;
                    }
                    else
                        noMatchCount++;

                    tryCount++;
                }

                if (multipleMatchesCount > noMatchCount)
                    seedLength++;
                else
                    seedLength /= 2;
            }

            return null;
        }

        private class AlignmentSeed
        {
            public AlignmentSeed(int sequenceToBeAlignedStartIndex, int referenceStartIndex, int seedLength)
            {
                SequenceToBeAlignedStartIndex = sequenceToBeAlignedStartIndex;
                ReferenceStartIndex = referenceStartIndex;
                SeedLength = seedLength;
            }

            public int SequenceToBeAlignedStartIndex { get; }
            public int ReferenceStartIndex { get; }
            public int SeedLength { get; }

        }

        private class UnalignedPart
        {
            public StringProxy ReferenceSequence { get; }
            public StringProxy SequenceToBeAligned { get; }

            public UnalignedPart(StringProxy sequenceToBeAligned, StringProxy referenceSequence)
            {
                SequenceToBeAligned = sequenceToBeAligned;
                ReferenceSequence = referenceSequence;
            }
        }

        private enum SequenceType
        {
            ToBeAligned,
            Reference
        }
    }
}