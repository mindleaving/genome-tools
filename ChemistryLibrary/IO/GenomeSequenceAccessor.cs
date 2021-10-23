using System;
using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequenceAccessor
    {
        private readonly string filePath;
        private readonly Dictionary<string, int> sequenceFileOffsets;

        public GenomeSequenceAccessor(string filePath)
        {
            this.filePath = filePath;
        }

        public string GetSequence(string sequenceId, int startIndex, int endIndex)
        {
            throw new NotImplementedException();
        }
    }
}