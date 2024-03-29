﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GenomeTools.ChemistryLibrary.Genomics;
using NUnit.Framework;

namespace GenomeTools.Tools
{
    public class SequenceEditing
    {
        [Test]
        [TestCase("GGACUUGAAA")]
        public void RunInvertRNA(string sequenceOrFile)
        {
            var sequence = TryLoadOrReturnSequence(sequenceOrFile);
            var invertedSequence = new string(sequence.Select(SequenceInverter.InvertRNA).ToArray());
            PrintTwoSequences(sequence, invertedSequence);
        }

        [Test]
        [TestCase("GGACTTGAAA")]
        public void RunInvertDNA(string sequenceOrFile)
        {
            var sequence = TryLoadOrReturnSequence(sequenceOrFile);
            var invertedSequence = new string(sequence.Select(SequenceInverter.InvertDNA).ToArray());
            PrintTwoSequences(sequence, invertedSequence);
        }

        [Test]
        [TestCase(@"F:\datasets\Biochemie_Versuch2\Sequence_pEYEFP-C3-TPX2.txt")]
        public void ComplementaryDNA(string sequenceOrFile)
        {
            var sequence = TryLoadOrReturnSequence(sequenceOrFile);
            var complementarySequence = new string(sequence.Select(SequenceInverter.InvertDNA).Reverse().ToArray());
            PrintTwoSequences(sequence, complementarySequence);
        }

        [Test]
        [TestCase("GGACUUGAAA")]
        public void RNAtoDNA(string sequenceOrFile)
        {
            var sequence = TryLoadOrReturnSequence(sequenceOrFile);
            var translated = sequence.Replace('U', 'T');
            PrintTwoSequences(sequence, translated);
        }

        [Test]
        [TestCase("CAUCAGCACCG")]
        public void DNAtoRNA(string sequenceOrFile)
        {
            var sequence = TryLoadOrReturnSequence(sequenceOrFile);
            var translated = sequence.Replace('T', 'U');
            PrintTwoSequences(sequence, translated);
        }

        [Test]
        [TestCase("CATCAGCACCGT")]
        public void Reverse(string sequenceOrFile)
        {
            var sequence = TryLoadOrReturnSequence(sequenceOrFile);
            var reversed = new string(sequence.Reverse().ToArray());
            PrintTwoSequences(sequence, reversed);
        }

        private static string TryLoadOrReturnSequence(string sequenceOrFile)
        {
            return File.Exists(sequenceOrFile) ? Regex.Replace(File.ReadAllText(sequenceOrFile), "\\s", "") : sequenceOrFile;
        }

        private static void PrintTwoSequences(string sequence, string invertedSequence)
        {
            Console.WriteLine($"Sequence: {sequence}");
            Console.WriteLine($"Modified: {invertedSequence}");
        }
    }
}
