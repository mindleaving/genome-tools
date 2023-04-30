using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO;

namespace GenomeTools.Studies
{
    public static class GenomeAlignmentPrinter
    {
        public static void Print(GenomeSequenceAlignment alignment, PrintTarget printTarget, string filePath = null)
        {
            if (printTarget == PrintTarget.File && filePath == null)
                throw new ArgumentNullException(nameof(filePath), "File path must be specified when printing to file");
            if(printTarget == PrintTarget.File && File.Exists(filePath))
                File.Delete(filePath);
            var readStartIndex = alignment.Reads.Where(x => x.IsMapped).Min(x => x.ReferenceStartIndex.Value);
            var readEndIndex = alignment.Reads.Where(x => x.IsMapped).Max(x => x.ReferenceEndIndex.Value);
            var readLength = readEndIndex - readStartIndex + 1;
            var referenceLine = new char[readLength];
            Array.Fill(referenceLine, ' ');
            var consensusLine = new char[readLength];
            Array.Fill(consensusLine, ' ');
            var secondaryConsensusLine = new char[readLength];
            Array.Fill(secondaryConsensusLine, ' ');
            var diffLine = new char[readLength];
            Array.Fill(diffLine, ' ');
            var referenceSequence = alignment.ReferenceSequence.GetSequence();
            var consensusSequence = alignment.AlignmentSequence.PrimarySequence.GetSequence();
            if (consensusSequence.Length != alignment.ReferenceSequence.Length)
                throw new Exception("Consensus sequence has an unexpected length. Expected length equal to range spanned by all mapped reads");
            var secondaryConsensusSequence = alignment.AlignmentSequence.SecondarySequence.GetSequence();
            if (secondaryConsensusSequence.Length != alignment.ReferenceSequence.Length)
                throw new Exception("Secondary consensus sequence has an unexpected length. Expected length equal to range spanned by all mapped reads");
            for (int referencePosition = readStartIndex; referencePosition <= readEndIndex; referencePosition++)
            {
                var localIndex = referencePosition - readStartIndex;
                if (referencePosition >= alignment.StartIndex && referencePosition <= alignment.EndIndex)
                {
                    var localReferenceIndex = referencePosition - alignment.StartIndex;
                    referenceLine[localIndex] = referenceSequence[localReferenceIndex];
                    consensusLine[localIndex] = consensusSequence[localReferenceIndex];
                    secondaryConsensusLine[localIndex] = secondaryConsensusSequence[localReferenceIndex];
                    if (referenceLine[localIndex] != consensusLine[localIndex] 
                        || referenceLine[localIndex] != secondaryConsensusLine[localIndex])
                    {
                        diffLine[localIndex] = '!';
                    }
                }
            }

            WriteLine($"REF:  {new string(referenceLine)}", printTarget, filePath);
            WriteLine($"OWN:  {new string(consensusLine)}", printTarget, filePath);
            WriteLine($"OWN2: {new string(secondaryConsensusLine)}", printTarget, filePath);
            WriteLine($"DIF:  {new string(diffLine)}", printTarget, filePath);
            var readLines = new List<char[]>();
            foreach (var read in alignment.Reads.Where(x => x.IsMapped))
            {
                var referenceAlignedSequence = read.GetReferenceAlignedSequence();
                var readLine = TryFindReadLineWithEnoughSpace(readLines, readStartIndex, read.ReferenceStartIndex.Value, read.ReferenceEndIndex.Value);
                if (readLine == null)
                {
                    readLine = new char[readLength];
                    Array.Fill(readLine, ' ');
                    readLines.Add(readLine);
                }
                for (int i = readStartIndex; i <= readEndIndex; i++)
                {
                    if (i >= read.ReferenceStartIndex.Value && i <= read.ReferenceEndIndex.Value)
                        readLine[i - readStartIndex] = referenceAlignedSequence[i - read.ReferenceStartIndex.Value];
                }
            }

            var lineIndex = 0;
            foreach (var readLine in readLines)
            {
                WriteLine($"{lineIndex:000}:  {new string(readLine)}", printTarget, filePath);
                lineIndex++;
            }
        }

        private static void WriteLine(string str, PrintTarget printTarget, string filePath)
        {
            switch (printTarget)
            {
                case PrintTarget.Console:
                    Console.WriteLine(str);
                    break;
                case PrintTarget.File:
                    File.AppendAllLines(filePath, new []{ str });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(printTarget), printTarget, null);
            }
        }

        public enum PrintTarget
        {
            Console,
            File
        }

        private static char[] TryFindReadLineWithEnoughSpace(
            List<char[]> readLines, 
            int referenceStartIndex, 
            int readReferenceStartIndex,
            int readReferenceEndIndex)
        {
            var readLength = readReferenceEndIndex - readReferenceStartIndex + 1;
            return readLines.FirstOrDefault(line => string.IsNullOrWhiteSpace(new string(line).Substring(readReferenceStartIndex-referenceStartIndex, readLength)));
        }
    }
}