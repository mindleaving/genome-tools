using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequence
    {
        public GenomeSequence(string sequence, int referenceStartIndex, int referenceEndIndex)
            : this(new List<GenomeSequencePart>
            {
                GenomeSequencePart.MatchedBases(new VerbatimGenomeReadSequence(sequence), referenceStartIndex, referenceEndIndex)
            })
        {
        }
        public GenomeSequence(IEnumerable<GenomeSequencePart> parts)
        {
            Parts = parts.OrderBy(x => x.ReferenceStartIndex).ToList();
        }

        public IReadOnlyList<GenomeSequencePart> Parts { get; }

        public string GetSequence()
        {
            var sequenceBuilder = new StringBuilder();
            foreach (var part in Parts)
            {
                var referenceLength = part.ReferenceEndIndex - part.ReferenceStartIndex + 1;
                var sequence = part.Sequence?.GetSequence();
                switch (part.Type)
                {
                    case GenomeSequencePartType.MatchedBases:
                        sequenceBuilder.Append(sequence);
                        break;
                    case GenomeSequencePartType.Insert:
                        sequenceBuilder.Append(sequence);
                        break;
                    case GenomeSequencePartType.Deletion:
                        sequenceBuilder.Append(Enumerable.Repeat('-', referenceLength));
                        break;
                    case GenomeSequencePartType.Reversal:
                        sequenceBuilder.Append(sequence);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return sequenceBuilder.ToString();
        }

        /// <summary>
        /// Build a mask that indicates where the reference must have accomodate for inserts,
        /// e.g. NNNNNNNN------NNNNNNNNN where N is a base from the reference and - indicates an insert.
        /// This is guaranteed to have the same length as the result from <see cref="GetSequence"/>
        /// </summary>
        public string GetReferenceMask()
        {
            var referenceMaskBuilder = new StringBuilder();
            foreach (var part in Parts)
            {
                var referenceLength = part.ReferenceEndIndex - part.ReferenceStartIndex + 1;
                var sequence = part.Sequence?.GetSequence();
                switch (part.Type)
                {
                    case GenomeSequencePartType.MatchedBases:
                        referenceMaskBuilder.Append(Enumerable.Repeat('N', referenceLength));
                        break;
                    case GenomeSequencePartType.Insert:
                        referenceMaskBuilder.Append(Enumerable.Repeat('-', sequence.Length));
                        break;
                    case GenomeSequencePartType.Deletion:
                        referenceMaskBuilder.Append(Enumerable.Range('N', referenceLength));
                        break;
                    case GenomeSequencePartType.Reversal:
                        referenceMaskBuilder.Append(Enumerable.Range('N', referenceLength));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return referenceMaskBuilder.ToString();
        }
    }
}