using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Commons;
using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class RansDecoderTest
    {
        [Test]
        public void CanDecodeOrder0()
        {
            byte order = 0;
            var frequencyTable = new byte[]
            {
                0, 133, 86, 49, 130, 170, 50, 0
            };
            var compressedData = new byte[]
            {
                130, 170, 114, 133, 85, 0, 212, 42, 5, 27, 145, 85, 5, 27, 136, 98, 5, 27, 126, 45, 5, 27
            };
            uint compressedSize = (uint)(frequencyTable.Length + compressedData.Length);
            uint uncompressedSize = 12;
            var inputBytes = new byte[] { order }
                .Concat(BitConverter.GetBytes(compressedSize))
                .Concat(BitConverter.GetBytes(uncompressedSize))
                .Concat(frequencyTable)
                .Concat(compressedData)
                .ToArray();
            var input = new MemoryStream(inputBytes);
            var output = new MemoryStream();

            RansDecoder.Decode(input, output);

            var actual = output.ToArray();
            Assert.That(actual, Is.EqualTo(new byte[] { 114, 49, 0, 114, 49, 0, 114, 50, 0, 114, 50, 0 }));
        }

        [Test]
        public void CanDecodeOrder1()
        {
            byte order = 1;
            var frequencyTable = new byte[]
            {
                0, 114, 143, 255, 0, 49, 0, 143, 255, 0, 50, 0, 0, 143, 255, 0, 114, 49, 135, 255, 50, 0, 136, 0, 0, 0
            };
            var compressedData = new byte[]
            {
                7, 64, 0, 1, 7, 64, 0, 1, 1, 40, 0, 1, 1, 40, 0, 1
            };
            uint compressedSize = (uint)(frequencyTable.Length + compressedData.Length);
            uint uncompressedSize = 12;
            var inputBytes = new byte[] { order }
                .Concat(BitConverter.GetBytes(compressedSize))
                .Concat(BitConverter.GetBytes(uncompressedSize))
                .Concat(frequencyTable)
                .Concat(compressedData)
                .ToArray();
            var input = new MemoryStream(inputBytes);
            var output = new MemoryStream();

            RansDecoder.Decode(input, output);

            var actual = output.ToArray();
            Assert.That(actual, Is.EqualTo(new byte[] { 114, 49, 0, 114, 49, 0, 114, 50, 0, 114, 50, 0 }));
        }

        [Test]
        //[Ignore("Tool")]
        public void CompareByteAndStreamSeekWriteSpeed()
        {
            var iterations = 1_000_000;
            var arrayLength = 400;
            var dataToWrite = Enumerable.Range(0, iterations).Select(_ => (byte)StaticRandom.Rng.Next(byte.MaxValue)).ToList();
            var indices = Enumerable.Range(0, iterations).Select(_ => StaticRandom.Rng.Next(arrayLength)).ToList();

            var array = new byte[arrayLength];
            var arrayStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var number = dataToWrite[i];
                var index = indices[i];
                array[index] = number;
            }
            arrayStopwatch.Stop();
            var arrayTime = arrayStopwatch.Elapsed;

            var stream = new MemoryStream(new byte[arrayLength]);
            var streamStopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var number = dataToWrite[i];
                var index = indices[i];
                stream.Seek(index, SeekOrigin.Begin);
                stream.WriteByte(number);
            }
            stream.Close();
            streamStopwatch.Stop();
            var streamTime = streamStopwatch.Elapsed;

            Console.WriteLine($"Array write time for {iterations:n0} items: {arrayTime.TotalMilliseconds} ms");
            Console.WriteLine($"MemoryStream write time for {iterations:n0} items: {streamTime.TotalMilliseconds} ms");

            // Result:
            // Array write time for 1,000,000 items: 2.1772 ms
            // MemoryStream write time for 1,000,000 items: 7.881 ms
        }

        private static readonly string[] DebugFilePaths = Directory.GetFiles(@"C:\temp\RansDecoder", "*.bin");
        [Test]
        [TestCaseSource(nameof(DebugFilePaths))]
        public void Debug(string filePath)
        {
            var input = File.OpenRead(filePath);
            var output = new MemoryStream();

            try
            {
                RansDecoder.Decode(input, output);
            }
            catch
            {
                var bytes = output.ToArray();
                File.WriteAllBytes(filePath + ".out", bytes);
            }
        }
    }
}
