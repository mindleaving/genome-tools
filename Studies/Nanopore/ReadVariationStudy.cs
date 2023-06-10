using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Bam;
using GenomeTools.ChemistryLibrary.IO.Fastq;
using ICSharpCode.SharpZipLib.GZip;
using NUnit.Framework;

namespace GenomeTools.Studies.Nanopore
{
    internal class ReadVariationStudy
    {
        [Test]
        public void BaseStatisticsFastq()
        {
            var fastqDirectory = @"F:\datasets\WFT_Nanopore\2023-04-13\18S_IVT_rRNA\fastq_pass";
            var files = Directory.EnumerateFiles(fastqDirectory);
            var fastqReader = new FastqReader();
            foreach (var file in files)
            {
                using var fileStream = File.OpenRead(file);
                using var memoryStream = new MemoryStream();
                GZip.Decompress(fileStream, memoryStream, false);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var reads = fastqReader.Read(memoryStream);
                foreach (var read in reads)
                {
                    Console.WriteLine(read.GetSequence());
                }
            }
        }

        [Test]
        [TestCase(@"F:\datasets\WFT_Nanopore\alignments\18S_IVT_2023-03-28.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\18S_IVT_rRNA\18S_IVT_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\alignments\WT_ctrl_2023-03-28.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\WT1_14_ctrl_1_rRNA\WT1_14_ctrl_1_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_A_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_J1_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_V_rRNA.bam", 0)]
        //[TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\WT1_14_ctrl_1_rRNA\WT1_14_ctrl_1_rRNA.bam", 1)]
        public void ReadLengthStatistics(string bamFilePath, int referenceIndex)
        {
            var referenceFilePath = @"F:\datasets\WFT_Nanopore\references\reference.fasta";

            var bamReader = new BamReader();
            var statistics = new StdDevAggregator();
            var readsLengths = new List<int>();
            bamReader.Load(
                bamFilePath,
                referenceFilePath,
                read =>
                {
                    if (!read.IsMapped)
                        return;
                    if(read.ReferenceId != referenceIndex)
                        return;

                    // Show aligned reads
                    //Console.WriteLine(new string(' ', read.ReferenceStartIndex.Value) + read.GetReferenceAlignedSequence());

                    readsLengths.Add(read.Length);
                    statistics.Add(read.Length);
                });
            Console.WriteLine("Read lengths");
            Console.WriteLine($"Mean: {statistics.Mean:F1}");
            Console.WriteLine($"Median: {readsLengths.Median(x => x):F0}");
            Console.WriteLine($"Std.Dev.: {statistics.SampleStddev:F2}");
            Console.WriteLine($"Raw:{string.Join(",", readsLengths)}");
        }

        [Test]
        [TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\18S_IVT_rRNA\18S_IVT_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\WT1_14_ctrl_1_rRNA\WT1_14_ctrl_1_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_A_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_J1_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_V_rRNA.bam", 0)]
        //[TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\WT1_14_ctrl_1_rRNA\WT1_14_ctrl_1_rRNA.bam", 1)]
        public void CalculateBaseStatistics(string bamFilePath, int referenceIndex)
        {
            var referenceFilePath = @"F:\datasets\WFT_Nanopore\references\reference.fasta";

            var referenceAccessor = new GenomeSequenceAccessor(referenceFilePath);
            var referenceSequence = referenceAccessor.GetSequenceByName("NR_003286_RNA18SN5");
            var referenceLength = referenceSequence.Length;
            var bamReader = new BamReader();
            var baseStatistics = Enumerable.Range(0, referenceLength).Select(_ => new BaseStatistics()).ToList();

            void AnalyzeBases(
                GenomeRead read)
            {
                if (!read.IsMapped) return;
                if (read.ReferenceId != referenceIndex) return;
                //if (read.Id == "1879b89c-b34b-4a3e-95b0-1aa693712c4e")
                //    Debugger.Break();

                for (var i = read.ReferenceStartIndex.Value; i <= read.ReferenceEndIndex.Value; i++)
                {
                    var currentBase = read.GetBaseAtReferencePosition(i);
                    var currentBaseStats = baseStatistics[i];
                    currentBaseStats.ReadCount++;
                    var featuresAtPosition = read.GetFeaturesAtReferencePosition(i).ToList();
                    if (featuresAtPosition.Any(x => x.Type == GenomeSequencePartType.Insertion))
                        currentBaseStats.InsertionCount++;
                    if (featuresAtPosition.Any(x => x.Type == GenomeSequencePartType.SoftClip))
                        currentBaseStats.SoftClipCount++;
                    if (featuresAtPosition.Any(x => x.Type == GenomeSequencePartType.ReferenceSkip))
                        currentBaseStats.SkipCount++;
                    if (featuresAtPosition.Any(x => x.Type == GenomeSequencePartType.Deletion)) 
                        currentBaseStats.DeletionCount++;
                    if (read.ReferenceStartIndex.HasValue && read.ReferenceStartIndex == i)
                        currentBaseStats.ReadStartCount++;
                    if (read.ReferenceEndIndex.HasValue && read.ReferenceEndIndex == i)
                        currentBaseStats.ReadEndCount++;
                    switch (currentBase)
                    {
                        case 'A':
                            currentBaseStats.ACount++;
                            break;
                        case 'C':
                            currentBaseStats.CCount++;
                            break;
                        case 'G':
                            currentBaseStats.GCount++;
                            break;
                        case 'T':
                        case 'U':
                            currentBaseStats.TCount++;
                            break;
                    }
                }
            }

            bamReader.Load(
                bamFilePath,
                referenceFilePath,
                AnalyzeBases);

            Console.WriteLine("Index;Reference;Coverage;A;C;G;T;Insert;Deletion;Skip;SoftClip;ReadStart;ReadEnd");
            for (var i = 0; i < baseStatistics.Count; i++)
            {
                var baseStatistic = baseStatistics[i];
                var referenceBase = referenceSequence.GetBaseAtPosition(i);
                Console.WriteLine($"{i};{referenceBase};{baseStatistic.ReadCount};{baseStatistic.ACount};{baseStatistic.CCount};{baseStatistic.GCount};{baseStatistic.TCount};{baseStatistic.InsertionCount};{baseStatistic.DeletionCount};{baseStatistic.SkipCount};{baseStatistic.SoftClipCount};{baseStatistic.ReadStartCount};{baseStatistic.ReadEndCount}");
            }
        }

     
        private class BaseStatistics
        {
            public int ReadCount { get; set; }
            public int ACount { get; set; }
            public int CCount { get; set; }
            public int GCount { get; set; }
            public int TCount { get; set; }
            public int DeletionCount { get; set; }
            public int InsertionCount { get; set; }
            public int SkipCount { get; set; }
            public int SoftClipCount { get; set; }
            public int ReadStartCount { get; set; }
            public int ReadEndCount { get; set; }
        }

        [Test]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_A_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_J1_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\20230509_WFT_blood_samples\blood_V_rRNA.bam", 0)]
        public void GetReadStartEndPositions(
            string bamFilePath,
            int referenceIndex)
        {
            var referenceFilePath = @"F:\datasets\WFT_Nanopore\references\reference.fasta";
            var bamReader = new BamReader();

            var readSpans = new List<Range<int>>();

            void AnalyzeBases(
                GenomeRead read)
            {
                if (!read.IsMapped) return;
                if (read.ReferenceId != referenceIndex) return;
                
                readSpans.Add(new Range<int>(read.ReferenceStartIndex!.Value, read.ReferenceEndIndex!.Value));
            }

            bamReader.Load(
                bamFilePath,
                referenceFilePath,
                AnalyzeBases);

            foreach (var readSpan in readSpans.OrderBy(x => x.From))
            {
                Console.WriteLine($"{readSpan.From};{readSpan.To}");
            }
        }

        [Test]
        [TestCase(1,7302)]
        public void GetRandomSequence(int startIndex, int endIndex)
        {
            var sequence = Enumerable.Range(1, endIndex - startIndex + 1)
                .Select(
                    index => new
                    {
                        Index = index,
                        RandomNumber = Random.Shared.NextDouble()
                    })
                .OrderBy(x => x.RandomNumber)
                .Select(x => x.Index);
            foreach (var i in sequence)
            {
                Console.WriteLine(i);
            }
        }
    }
}
