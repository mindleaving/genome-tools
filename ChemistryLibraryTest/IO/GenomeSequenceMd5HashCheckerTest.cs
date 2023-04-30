using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO;
using GenomeTools.ChemistryLibrary.IO.Fasta;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO
{
    public class GenomeSequenceMd5HashCheckerTest
    {
        private readonly string input = ">chr1\n"
                                        + "ACGTNNTGCA\n"
                                        + "CAGG\n"
                                        + "\n"
                                        + ">chr2\n";

        [Test]
        public void CheckMd5HashTrueForMatchingHash()
        {
            var indexEntry = new FastaIndexEntry("chr1", 6, 10, 11);
            var expectedMd5Hash = "750efd59e744d60a61b6059681b18bb5";
            var sut = new GenomeSequenceMd5HashChecker();

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
            var actual = sut.CheckMd5Hash(stream, indexEntry, expectedMd5Hash);
            Assert.That(actual, Is.True);
        }

        [Test]
        public void CheckMd5HashFalseForNonMatchingHash()
        {
            var indexEntry = new FastaIndexEntry("chr1", 6, 10, 11);
            var expectedMd5Hash = "6cd3556deb0da54bca060b4c39479839";
            var sut = new GenomeSequenceMd5HashChecker();

            var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
            var actual = sut.CheckMd5Hash(stream, indexEntry, expectedMd5Hash);
            Assert.That(actual, Is.False);
        }

        [Test]
        [Ignore("Test")]
        public void Md5HashTransformBlockTest()
        {
            // Test how to correctly compute MD5 hash

            var bytes = Encoding.ASCII.GetBytes("Hello, world!");
            var expectedBytes = ParserHelpers.ParseHexString("6cd3556deb0da54bca060b4c39479839");
            var md5Hash = MD5.Create();
            md5Hash.Initialize();
            md5Hash.TransformBlock(bytes, 0, 3, null, 0);
            md5Hash.TransformBlock(bytes, 3, 10, null, 0);
            md5Hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            var hash = md5Hash.Hash;
            Assert.That(hash, Is.EqualTo(expectedBytes));
        }
    }
}
