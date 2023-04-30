using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Fasta;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequenceAccessor : IGenomeSequenceAccessor, IDisposable
    {
        private readonly string sequenceFilePath;
        private readonly string indexFilePath;
        private readonly ReferenceSequenceMap sequenceNameOrder;
        private Dictionary<string, FastaIndexEntry> indexEntries;
        private Stream fileStream;
        private readonly bool useCaching;
        private string cachedSequence;
        private string cachedSequenceName;

        public GenomeSequenceAccessor(
            string sequenceFilePath,
            ReferenceSequenceMap sequenceNameOrder = null,
            bool useCaching = true)
        {
            this.sequenceFilePath = sequenceFilePath;
            indexFilePath = sequenceFilePath + ".fai";
            this.sequenceNameOrder = sequenceNameOrder;
            this.useCaching = useCaching;
        }

        private void Initialize()
        {
            if (indexEntries == null)
            {
                if (!File.Exists(indexFilePath))
                    BuildReferenceIndex();
                LoadReferenceIndex();
            }
            if(fileStream == null)
                fileStream = File.OpenRead(sequenceFilePath);
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

        public List<string> GetSequenceNames()
        {
            Initialize();
            return indexEntries.Keys.ToList();
        }

        public IGenomeSequence GetSequenceByName(string sequenceName, int startIndex = 0, int? endIndex = null)
        {
            Initialize();
            if (!indexEntries.ContainsKey(sequenceName))
                throw new KeyNotFoundException($"Sequence with the name '{sequenceName}' wasn't found in the reference");
            var indexEntry = indexEntries[sequenceName];
            if (useCaching)
            {
                if(cachedSequenceName != sequenceName)
                {
                    cachedSequence = LoadWholeSeqeunce(indexEntry);
                    cachedSequenceName = sequenceName;
                }

                var length = endIndex.HasValue 
                    ? Math.Min(endIndex.Value, cachedSequence.Length - 1) - startIndex + 1
                    : cachedSequence.Length - startIndex;
                return new GenomeSequence(cachedSequence.Substring(startIndex, length), sequenceName, startIndex);
            }

            var startLineNumber = startIndex / indexEntry.BasesPerLine;
            var startCharacterInLine = startIndex - indexEntry.BasesPerLine * startLineNumber;
            var startByteOffset = indexEntry.FirstBaseOffset + startLineNumber * indexEntry.LineWidth + startCharacterInLine;
            if (!endIndex.HasValue)
                endIndex = (int)indexEntry.Length-1;

            var sequenceBuilder = new StringBuilder();
            fileStream.Seek(startByteOffset, SeekOrigin.Begin);
            using var streamReader = new StreamReader(fileStream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
            var currentIndex = startIndex;
            while (currentIndex <= endIndex.Value)
            {
                var line = streamReader.ReadLine();
                if (line == null)
                    throw new Exception($"Could not find parts of sequence '{sequenceName}:{startIndex}:{endIndex}' in reference file");
                sequenceBuilder.Append(line.ToUpper());
                currentIndex += line.Length;
            }

            var sequence = sequenceBuilder.ToString().Substring(0, endIndex.Value - startIndex + 1);
            return new GenomeSequence(sequence, sequenceName, startIndex);
        }

        private string LoadWholeSeqeunce(FastaIndexEntry indexEntry)
        {
            var sequenceBuilder = new StringBuilder();
            fileStream.Seek(indexEntry.FirstBaseOffset, SeekOrigin.Begin);
            using var streamReader = new StreamReader(fileStream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true);
            var currentIndex = 0;
            while (currentIndex < indexEntry.Length)
            {
                var line = streamReader.ReadLine();
                if(line == null)
                    break;
                sequenceBuilder.Append(line.ToUpper());
                currentIndex += line.Length;
            }
            return sequenceBuilder.ToString();
        }

        public IGenomeSequence GetSequenceById(int referenceId, int startIndex = 0, int? endIndex = null)
        {
            var sequenceName = sequenceNameOrder.GetSequenceNameFromIndex(referenceId);
            return GetSequenceByName(sequenceName, startIndex, endIndex);
        }

        public void Dispose()
        {
            fileStream?.Dispose();
        }
    }
}