using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Commons.Extensions;
using Commons.Mathematics;
using GenomeTools.ChemistryLibrary.Genomics;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Bam;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Fastq;
using GenomeTools.ChemistryLibrary.IO.Sam;
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
        [TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\18S_IVT_rRNA\18S_IVT_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\WT1_14_ctrl_1_rRNA\WT1_14_ctrl_1_rRNA.bam", 0)]
        [TestCase(@"F:\datasets\WFT_Nanopore\2023-04-13\WT1_14_ctrl_1_rRNA\WT1_14_ctrl_1_rRNA.bam", 1)]
        public void BaseStatisticsBam(string bamFilePath, int referenceIndex)
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

        
    }
}
