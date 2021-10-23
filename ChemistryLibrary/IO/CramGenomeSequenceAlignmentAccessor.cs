﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Cram;
using GenomeTools.ChemistryLibrary.IO.Cram.Index;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Fasta;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class CramGenomeSequenceAlignmentAccessor : IGenomeAlignmentAccessor
    {
        private readonly string alignmentFilePath;
        private readonly string alignmentIndexFilePath;
        private CramIndex alignmentIndex;
        private readonly string referenceSequenceFilePath;
        private readonly string referenceSequenceIndexFilePath;
        private Dictionary<string, FastaIndexEntry> referenceIndex;
        private readonly CramLoader cramLoader;

        public CramGenomeSequenceAlignmentAccessor(string alignmentFilePath, string referenceSequenceFilePath)
        {
            this.alignmentFilePath = alignmentFilePath;
            alignmentIndexFilePath = alignmentFilePath + ".crai";
            this.referenceSequenceFilePath = referenceSequenceFilePath;
            referenceSequenceIndexFilePath = referenceSequenceFilePath + ".fai";
            cramLoader = new CramLoader();
        }


        public GenomeSequence GetReferenceSequence(string chromosome, int startIndex, int endIndex)
        {
            if (!File.Exists(referenceSequenceIndexFilePath))
                BuildReferenceIndex();
            if (referenceIndex == null)
                LoadReferenceIndex();
            if (!referenceIndex.ContainsKey(chromosome))
                throw new KeyNotFoundException($"Sequence with the name '{chromosome}' wasn't found in the reference");
            var indexEntry = referenceIndex[chromosome];
            var startLineNumber = startIndex / indexEntry.BasesPerLine;
            var startCharacterInLine = startIndex - indexEntry.BasesPerLine * startLineNumber;
            var startByteOffset = indexEntry.FirstBaseOffset + startLineNumber * indexEntry.LineWidth + startCharacterInLine;

            var sequenceBuilder = new StringBuilder();
            using var fileStream = File.OpenRead(referenceSequenceFilePath);
            fileStream.Seek(startByteOffset, SeekOrigin.Begin);
            using var streamReader = new StreamReader(fileStream);
            var currentIndex = startIndex;
            while (currentIndex < endIndex)
            {
                var line = streamReader.ReadLine();
                if (line == null)
                    throw new Exception($"Could not find parts of sequence '{chromosome}:{startIndex}:{endIndex}' in reference file");
                sequenceBuilder.Append(line);
                currentIndex += line.Length;
            }

            var sequence = sequenceBuilder.ToString().Substring(0, endIndex - startIndex + 1);
            return new GenomeSequence(sequence, startIndex, endIndex);
        }

        public GenomeSequence GetAlignmentSequence(string chromosome, int startIndex, int endIndex)
        {
            throw new NotImplementedException();
        }

        public GenomeSequenceAlignment GetAlignment(string chromosome, int startIndex, int endIndex)
        {
            var referenceSequence = GetReferenceSequence(chromosome, startIndex, endIndex);
            var alignmentSequence = GetAlignmentSequence(chromosome, startIndex, endIndex);
            var maskedReferenceSequence = GenomeSequenceExtensions.ApplyMask(referenceSequence.GetSequence(), alignmentSequence.GetReferenceMask());
            var reads = GetReadsInRange(chromosome, startIndex, endIndex);
            return new GenomeSequenceAlignment(
                chromosome,
                startIndex,
                endIndex,
                maskedReferenceSequence,
                alignmentSequence.GetSequence(),
                reads);
        }

        private List<GenomeRead> GetReadsInRange(string chromosome, int startIndex, int endIndex)
        {
            throw new NotImplementedException();
        }

        private void BuildReferenceIndex()
        {
            var indexBuilder = new FastaIndexBuilder();
            indexBuilder.BuildIndexAndWriteToFile(referenceSequenceFilePath);
        }

        private void LoadReferenceIndex()
        {
            var indexLoader = new FastaIndexReader();
            referenceIndex = indexLoader.ReadIndex(referenceSequenceIndexFilePath).ToDictionary(x => x.SequenceName, x => x);
        }

        private List<CramSlice> GetOverlappingSlices(int sequenceId, int startIndex, int endIndex)
        {
            if (alignmentIndex == null)
                LoadAlignmentIndex();
            var matchingIndexEntries = alignmentIndex.GetEntriesForReferenceSequence(sequenceId)
                .Where(x => Overlaps(x, startIndex, endIndex))
                .ToList();
            var reader = new CramReader(alignmentFilePath);
            return matchingIndexEntries
                .Select(indexEntry => cramLoader.LoadSlice(reader, indexEntry))
                .ToList();
        }

        private bool Overlaps(CramIndexEntry indexEntry, int startIndex, int endIndex)
        {
            var entryStartIndex = indexEntry.AlignmentStart;
            var entryEndIndex = indexEntry.AlignmentStart + indexEntry.AlignmentSpan - 1;

            if (startIndex > entryEndIndex)
                return false;
            if (entryStartIndex > endIndex)
                return false;
            return true;
        }

        private void LoadAlignmentIndex()
        {
            var cramIndexReader = new CramIndexLoader();
            alignmentIndex = cramIndexReader.Load(alignmentIndexFilePath);
        }
    }
}