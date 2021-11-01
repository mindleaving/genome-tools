using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Fasta;
using GenomeTools.ChemistryLibrary.IO.Sam;

namespace GenomeTools.ChemistryLibrary.IO
{
    public class GenomeSequenceMd5HashChecker
    {
        public bool CheckMd5Hash(string sequenceFilePath, FastaIndexEntry indexEntry, string expectedMd5Hash)
        {
            using var fileStream = File.OpenRead(sequenceFilePath);
            return CheckMd5Hash(fileStream, indexEntry, expectedMd5Hash);
        }

        public bool CheckMd5Hash(Stream stream, FastaIndexEntry indexEntry, string expectedMd5Hash)
        {
            var hash = CalculateMd5Hash(stream, indexEntry);
            var expectedMd5HashBytes = ParserHelpers.ParseHexString(expectedMd5Hash);
            return hash.SequenceEqual(expectedMd5HashBytes);
        }

        public bool CheckMd5Hash(ReferenceSequenceSamHeaderEntry referenceSequenceEntry)
        {
            if (referenceSequenceEntry.Md5Checksum == null)
                return true;
            if (!File.Exists(referenceSequenceEntry.StorageLocation))
                return false;

            // Check for .md5 file with pre-calculated hashes
            // If it exists use it to skip MD5-hash calculation
            var md5HashFilePath = referenceSequenceEntry.StorageLocation + ".md5";
            if (File.Exists(md5HashFilePath))
            {
                var md5HashFileReader = new FastaMd5HashFileReader();
                var md5HashEntries = md5HashFileReader.Read(md5HashFilePath);
                var matchingEntry = md5HashEntries.FirstOrDefault(x => x.SequenceName == referenceSequenceEntry.ReferenceSequenceName);
                if (matchingEntry != null)
                    return string.Equals(matchingEntry.Md5Hash, referenceSequenceEntry.Md5Checksum, StringComparison.CurrentCultureIgnoreCase);
            }

            // Check for index, create it if it doesn't exist and read it
            var indexFilePath = referenceSequenceEntry.StorageLocation + ".fai";
            if(!File.Exists(indexFilePath))
            {
                var indexBuilder = new FastaIndexBuilder();
                indexBuilder.BuildIndexAndWriteToFile(indexFilePath);
            }
            var indexReader = new FastaIndexReader();
            var index = indexReader.ReadIndex(indexFilePath);
            var indexEntry = index.FirstOrDefault(x => x.SequenceName == referenceSequenceEntry.ReferenceSequenceName);
            if (indexEntry == null)
                return false;

            // Calculate hash
            using var fileStream = File.OpenRead(referenceSequenceEntry.StorageLocation);
            var hash = CalculateMd5Hash(fileStream, indexEntry);

            // Write hash to not calculate it next time
            var hashString = string.Join("", hash.Select(x => x.ToString("X2")));
            var md5HashFileWriter = new FastaMd5HashFileWriter();
            md5HashFileWriter.Append(md5HashFilePath, new FastaSequenceMd5Hash(referenceSequenceEntry.ReferenceSequenceName, hashString));

            // Compare MD5-hash
            var expectedMd5HashBytes = ParserHelpers.ParseHexString(referenceSequenceEntry.Md5Checksum);
            return hash.SequenceEqual(expectedMd5HashBytes);
        }

        private static byte[] CalculateMd5Hash(Stream stream, FastaIndexEntry indexEntry)
        {
            stream.Seek(indexEntry.FirstBaseOffset, SeekOrigin.Begin);
            using var reader = new StreamReader(stream);

            var md5Hash = MD5.Create();
            md5Hash.Initialize();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith(">"))
                    break;
                var trimmedLine = line.TrimEnd().ToUpper();
                md5Hash.TransformBlock(
                    Encoding.ASCII.GetBytes(trimmedLine),
                    0,
                    trimmedLine.Length,
                    null,
                    0);
                if (trimmedLine.Length < indexEntry.BasesPerLine)
                    break;
            }
            md5Hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return md5Hash.Hash;
        }
    }
}
