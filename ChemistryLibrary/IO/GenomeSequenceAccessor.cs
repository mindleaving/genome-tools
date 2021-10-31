using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Fasta;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequenceAccessor
    {
        private readonly string sequenceFilePath;
        private readonly string indexFilePath;
        private readonly ReferenceSequenceMap sequenceNameOrder;
        private Dictionary<string, FastaIndexEntry> indexEntries;

        public GenomeSequenceAccessor(
            string sequenceFilePath,
            ReferenceSequenceMap sequenceNameOrder)
        {
            this.sequenceFilePath = sequenceFilePath;
            indexFilePath = sequenceFilePath + ".fai";
            this.sequenceNameOrder = sequenceNameOrder;
        }

        private void Initialize()
        {
            if(indexEntries != null)
                return;
            if (!File.Exists(indexFilePath))
                BuildReferenceIndex();
            if (indexEntries == null)
                LoadReferenceIndex();
        }

        private void BuildReferenceIndex()
        {
            var indexBuilder = new FastaIndexBuilder();
            indexBuilder.BuildIndexAndWriteToFile(sequenceFilePath);
        }
        private void LoadReferenceIndex()
        {
            var indexLoader = new FastaIndexReader();
            indexEntries = indexLoader.ReadIndex(indexFilePath).ToDictionary(x => x.SequenceName, x => x);
        }

        public IGenomeSequence GetSequenceByName(string sequenceName, int startIndex, int endIndex)
        {
            Initialize();
            if (!indexEntries.ContainsKey(sequenceName))
                throw new KeyNotFoundException($"Sequence with the name '{sequenceName}' wasn't found in the reference");
            var indexEntry = indexEntries[sequenceName];
            var startLineNumber = startIndex / indexEntry.BasesPerLine;
            var startCharacterInLine = startIndex - indexEntry.BasesPerLine * startLineNumber;
            var startByteOffset = indexEntry.FirstBaseOffset + startLineNumber * indexEntry.LineWidth + startCharacterInLine;

            var sequenceBuilder = new StringBuilder();
            using var fileStream = File.OpenRead(sequenceFilePath);
            fileStream.Seek(startByteOffset, SeekOrigin.Begin);
            using var streamReader = new StreamReader(fileStream);
            var currentIndex = startIndex;
            while (currentIndex < endIndex)
            {
                var line = streamReader.ReadLine();
                if (line == null)
                    throw new Exception($"Could not find parts of sequence '{sequenceName}:{startIndex}:{endIndex}' in reference file");
                sequenceBuilder.Append(line);
                currentIndex += line.Length;
            }

            var sequence = sequenceBuilder.ToString().Substring(0, endIndex - startIndex + 1);
            return new GenomeSequence(sequence, sequenceName, startIndex);
        }

        public IGenomeSequence GetSequenceById(int referenceId, int startIndex, int endIndex)
        {
            var sequenceName = sequenceNameOrder.GetSequenceNameFromIndex(referenceId);
            return GetSequenceByName(sequenceName, startIndex, endIndex);
        }
    }
}