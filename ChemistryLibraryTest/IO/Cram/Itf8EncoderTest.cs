using System.IO;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Cram;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Cram
{
    public class Itf8EncoderTest
    {
        [Test]
        [TestCase(int.MinValue)]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(124)]
        [TestCase(8161819)]
        [TestCase(int.MaxValue)]
        public void Roundtrip(int number)
        {
            var encoded = Itf8Encoder.ToItf8(number);
            var decoded = new BinaryReader(new MemoryStream(encoded)).ReadItf8();

            Assert.That(decoded, Is.EqualTo(number));
        }
    }
}
