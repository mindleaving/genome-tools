using System;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.Genomics
{
    public static class SequenceInverter
    {
        /// <summary>
        /// Returns the sequence where each base is replaced by the complementary base.
        /// NOTE: Is the input sequence is in the 5'->3' direction,
        /// the output will correspond to the complementary sequence in the 3'->5'-direction.
        /// Use <see cref="ComplementaryDNA"/> to get the complementary sequence in the 5'->3'-direction.
        /// </summary>
        public static string InvertDNA(string dnaSequence)
        {
            return new string(dnaSequence.Select(InvertDNA).ToArray());
        }

        /// <summary>
        /// Returns the complementary strand.
        /// If the input is in the 5'->3'-direction, the output will be as well.
        /// </summary>
        public static string ComplementaryDNA(string dnaSequence)
        {
            return new string(dnaSequence.Select(InvertDNA).Reverse().ToArray());
        }

        public static string InvertRNA(string rnaSequence)
        {
            return new string(rnaSequence.Select(InvertRNA).ToArray());
        }

        public static char InvertRNA(char code)
        {
            return char.ToUpper(code) switch
            {
                'G' => 'C',
                'C' => 'G',
                'A' => 'U',
                'U' => 'A',
                _ => throw new NotSupportedException()
            };
        }

        public static char InvertDNA(char code)
        {
            return char.ToUpper(code) switch
            {
                'G' => 'C',
                'C' => 'G',
                'A' => 'T',
                'T' => 'A',
                _ => throw new NotSupportedException()
            };
        }
    }
}
