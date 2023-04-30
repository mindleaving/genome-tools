using System;
using System.Linq;
using System.Text;

namespace GenomeTools.ChemistryLibrary.Extensions
{
    public static class GenomeSequenceExtensions
    {
        public static string ApplyMask(string referenceSequence, string mask)
        {
            var maskNCount = mask.Count(c => c == 'N');
            if (maskNCount != referenceSequence.Length)
            {
                throw new ArgumentException(
                    "Length of reference sequence doesn't fit mask. "
                    + $"Length of reference sequence: {referenceSequence.Length}, mask 'N'-positions: {maskNCount}");
            }

            var sequenceBuilder = new StringBuilder();
            var referenceIndex = 0;
            foreach (var maskCharacter in mask)
            {
                if (maskCharacter == 'N')
                {
                    sequenceBuilder.Append(referenceSequence[referenceIndex]);
                    referenceIndex++;
                }
                else
                {
                    sequenceBuilder.Append('-');
                }
            }
            return sequenceBuilder.ToString();
        }
    }
}
