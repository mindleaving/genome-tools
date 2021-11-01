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
            var referenceStartIndex = alignment.Reads.Where(x => x.IsMapped).Min(x => x.ReferenceStartIndex.Value);
            var referenceEndIndex = alignment.Reads.Where(x => x.IsMapped).Max(x => x.ReferenceEndIndex.Value);
            var length = referenceEndIndex - referenceStartIndex + 1;
            var referenceLine = new char[length];
            Array.Fill(referenceLine, ' ');
            var consensusLine = new char[length];
            Array.Fill(consensusLine, ' ');
            var diffLine = new char[length];
            Array.Fill(diffLine, ' ');
            var referenceSequence = alignment.ReferenceSequence.GetSequence();
            var consensusSequence = alignment.AlignmentSequence.GetSequence();
            if (consensusSequence.Length != length)
                throw new Exception("Consensus sequence has an unexpected length. Expected length equal to range spanned by all mapped reads");
            for (int referencePosition = referenceStartIndex; referencePosition <= referenceEndIndex; referencePosition++)
            {
                var localIndex = referencePosition - referenceStartIndex;
                consensusLine[localIndex] = consensusSequence[localIndex];
                if (referencePosition >= alignment.StartIndex && referencePosition <= alignment.EndIndex)
                {
                    referenceLine[localIndex] = referenceSequence[referencePosition - alignment.StartIndex];
                    if (referenceLine[localIndex] != consensusLine[localIndex])
                        diffLine[localIndex] = '!';
                }
            }

            WriteLine($"REF: {new string(referenceLine)}", printTarget, filePath);
            WriteLine($"OWN: {new string(consensusLine)}", printTarget, filePath);
            WriteLine($"DIF: {new string(diffLine)}", printTarget, filePath);
            var readLines = new List<char[]>();
            foreach (var read in alignment.Reads.Where(x => x.IsMapped))
            {
                var referenceAlignedSequence = read.GetReferenceAlignedSequence();
                var readLine = TryFindReadLineWithEnoughSpace(readLines, referenceStartIndex, read.ReferenceStartIndex.Value, read.ReferenceEndIndex.Value);
                if (readLine == null)
                {
                    readLine = new char[length];
                    Array.Fill(readLine, ' ');
                    readLines.Add(readLine);
                }
                for (int i = referenceStartIndex; i <= referenceEndIndex; i++)
                {
                    if (i >= read.ReferenceStartIndex.Value && i <= read.ReferenceEndIndex.Value)
                        readLine[i - referenceStartIndex] = referenceAlignedSequence[i - read.ReferenceStartIndex.Value];
                }
            }

            var lineIndex = 0;
            foreach (var readLine in readLines)
            {
                WriteLine($"{lineIndex:000}: {new string(readLine)}", printTarget, filePath);
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