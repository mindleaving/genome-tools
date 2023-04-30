using System.Collections.Generic;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.Genomics;

namespace GenomeTools.ChemistryLibrary.IO.Bam
{
    public class BamCigarToReadFeatureConverter
    {
        public List<GenomeReadFeature> Convert(
            uint[] cigarOperations,
            IList<char> sequence,
            IList<char> qualityScores)
        {
            var features = new List<GenomeReadFeature>();
            var sequenceIndex = 0;
            var referenceIndex = 0;
            foreach (var cigarOperation in cigarOperations)
            {
                var cigarCode = cigarOperation & 0xf;
                int cigarOperationLength = (int)(cigarOperation >> 4); // TODO: Debug
                switch (cigarCode)
                {
                    case 0: // M
                    case 7: // = - Sequence match
                    case 8: // X - Sequence mismatch
                    {
                        var featureSequence = sequence.SubArray(sequenceIndex, cigarOperationLength);
                        var featureQualityScores = qualityScores.SubArray(sequenceIndex, cigarOperationLength);
                        features.Add(new GenomeReadFeature(GenomeSequencePartType.Bases, sequenceIndex, featureSequence, featureQualityScores));
                        sequenceIndex += cigarOperationLength;
                        referenceIndex += cigarOperationLength;
                        break;
                    }
                    case 1: // I
                    {
                        var featureSequence = sequence.SubArray(sequenceIndex, cigarOperationLength);
                        var featureQualityScores = qualityScores.SubArray(sequenceIndex, cigarOperationLength);
                        features.Add(new GenomeReadFeature(GenomeSequencePartType.Insertion, sequenceIndex, featureSequence, featureQualityScores));
                        sequenceIndex += cigarOperationLength;
                        break;
                    }
                    case 2: // D
                    {
                        features.Add(new GenomeReadFeature(GenomeSequencePartType.Deletion, sequenceIndex, deletionLength: cigarOperationLength));
                        referenceIndex += cigarOperationLength;
                        break;
                    }
                    case 3: // N
                    {
                        features.Add(new GenomeReadFeature(GenomeSequencePartType.ReferenceSkip, sequenceIndex, skipLength: cigarOperationLength));
                        referenceIndex += cigarOperationLength;
                        break;
                    }
                    case 4: // S
                    {
                        var featureSequence = sequence.SubArray(sequenceIndex, cigarOperationLength);
                        var featureQualityScores = qualityScores.SubArray(sequenceIndex, cigarOperationLength);
                        features.Add(new GenomeReadFeature(GenomeSequencePartType.SoftClip, sequenceIndex, featureSequence, featureQualityScores));
                        sequenceIndex += cigarOperationLength;
                        break;
                    }
                    case 5: // H
                    {
                        features.Add(new GenomeReadFeature(GenomeSequencePartType.HardClip, sequenceIndex));
                        break;
                    }
                    case 6: // P
                    {
                        features.Add(new GenomeReadFeature(GenomeSequencePartType.Padding, sequenceIndex));
                        break;
                    }
                }
            }
            return features;
        }
    }
}
