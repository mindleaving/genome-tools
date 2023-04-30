using System;
using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class BaseSubstitutionMatrix
    {
        private readonly Dictionary<byte, char> aSubstituions;
        private readonly Dictionary<byte, char> cSubstituions;
        private readonly Dictionary<byte, char> gSubstituions;
        private readonly Dictionary<byte, char> tSubstituions;
        private readonly Dictionary<byte, char> nSubstituions;

        public BaseSubstitutionMatrix(byte[] substitutionMatrixBytes)
        {
            aSubstituions = CreateSubstitutionLookup(substitutionMatrixBytes[0], "CGTN");
            cSubstituions = CreateSubstitutionLookup(substitutionMatrixBytes[1], "AGTN");
            gSubstituions = CreateSubstitutionLookup(substitutionMatrixBytes[2], "ACTN");
            tSubstituions = CreateSubstitutionLookup(substitutionMatrixBytes[3], "ACGN");
            nSubstituions = CreateSubstitutionLookup(substitutionMatrixBytes[4], "ACGT");
        }

        private static Dictionary<byte, char> CreateSubstitutionLookup(byte substitutionMatrixByte, string replacementBases)
        {
            var lookup = new Dictionary<byte, char>();
            for (int i = 0; i < 4; i++)
            {
                var bits = (substitutionMatrixByte >> (2 * (3 - i))) & 0x3;
                var replacement = replacementBases[i];
                lookup.Add((byte)bits, replacement);
            }
            return lookup;
        }

        public char Substitute(char referenceBase, byte substitutionCode)
        {
            switch (char.ToUpper(referenceBase))
            {
                case 'A':
                    return aSubstituions[substitutionCode];
                case 'C':
                    return cSubstituions[substitutionCode];
                case 'G':
                    return gSubstituions[substitutionCode];
                case 'T':
                    return tSubstituions[substitutionCode];
                case 'N':
                    return nSubstituions[substitutionCode];
                default:
                    return 'N';
                    //throw new ArgumentOutOfRangeException(nameof(referenceBase), $"Invalid reference base '{referenceBase}' for substitution. Supported: ACGTN.");
            }
        }

    }
}