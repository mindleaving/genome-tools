using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Sam;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramHeaderReader
    {
        private readonly Md5CheckFailureMode md5CheckFailureMode;

        public enum Md5CheckFailureMode
        {
            ThrowException,
            WriteToConsole,
            Ignore
        }

        public CramHeaderReader(Md5CheckFailureMode md5CheckFailureMode = Md5CheckFailureMode.ThrowException)
        {
            this.md5CheckFailureMode = md5CheckFailureMode;
        }

        public CramHeader Read(string filePath, string referenceSequenceFilePath = null)
        {
            using var reader = new CramBinaryReader(filePath);
            reader.CheckFileFormat();
            return Read(reader, reader.Position, referenceSequenceFilePath);
        }

        public CramHeader Read(CramBinaryReader reader, long offset, string referenceSequenceFilePath = null)
        {
            var samHeaderParser = new SamHeaderParser();

            // TODO: Move to other class
            var containerHeaderReader = new CramContainerHeaderReader();
            var containerHeader = containerHeaderReader.Read(reader, offset);

            var blockReader = new CramBlockReader();
            var blocks = blockReader.ReadBlocks(reader, containerHeader.NumberOfBlocks, null);
            var samHeader = Encoding.ASCII.GetString(blocks[0].UncompressedDecodedData);
            samHeader = samHeader.Substring(4); // TODO: Why are there 4 bytes before the header entries? String length?
            var samHeaderLines = samHeader.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var samHeaderEntries = samHeaderLines
                .Where(line => line.StartsWith("@"))
                .Select(samHeaderParser.Parse)
                .ToList();
            var referenceSequenceEntries = samHeaderEntries
                .Where(x => x.Type == SamHeaderEntry.HeaderEntryType.ReferenceSequence)
                .Cast<ReferenceSequenceSamHeaderEntry>()
                .ToList();
            if (referenceSequenceFilePath != null)
            {
                foreach (var referenceSequenceEntry in referenceSequenceEntries)
                {
                    referenceSequenceEntry.StorageLocation = referenceSequenceFilePath;
                }
            }

            CheckMd5Hashes(referenceSequenceEntries);
            return new CramHeader(samHeaderEntries);
        }

        private void CheckMd5Hashes(List<ReferenceSequenceSamHeaderEntry> referenceSequenceEntries)
        {
            var hashChecker = new GenomeSequenceMd5HashChecker();
            foreach (var referenceSequenceEntry in referenceSequenceEntries)
            {
                if (!File.Exists(referenceSequenceEntry.StorageLocation))
                {
                    var errorMessage = $"Path to reference sequence '{referenceSequenceEntry.ReferenceSequenceName}' wasn't found. Path: {referenceSequenceEntry.StorageLocation}";
                    HandleMd5CheckFailure(errorMessage);
                }

                if (!hashChecker.CheckMd5Hash(referenceSequenceEntry))
                {
                    var errorMessage = $"MD5-hash of reference sequence '{referenceSequenceEntry.ReferenceSequenceName}' "
                                                                          + "in the header doesn't match the data in the reference file";
                    HandleMd5CheckFailure(errorMessage);
                }
            }
        }

        private void HandleMd5CheckFailure(string errorMessage)
        {
            switch (md5CheckFailureMode)
            {
                case Md5CheckFailureMode.ThrowException:
                    throw new Exception(errorMessage);
                case Md5CheckFailureMode.WriteToConsole:
                    Console.WriteLine($"WARNING - {errorMessage}");
                    break;
                case Md5CheckFailureMode.Ignore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
