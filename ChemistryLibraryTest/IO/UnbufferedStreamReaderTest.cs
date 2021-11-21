using System.IO;
using System.Text;
using GenomeTools.ChemistryLibrary.IO;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO
{
    public class UnbufferedStreamReaderTest
    {
        [Test]
        public void ReturnsNullWhenStreamExhausted()
        {
            var input = "Line1\nLine2";
            var inputStream = new MemoryStream(Encoding.ASCII.GetBytes(input));

            var sut = new UnbufferedStreamReader(inputStream);

            Assert.That(sut.Position, Is.EqualTo(0));
            Assert.That(sut.ReadLine(), Is.EqualTo("Line1"));
            Assert.That(sut.Position, Is.EqualTo(6));
            Assert.That(sut.ReadLine(), Is.EqualTo("Line2"));
            Assert.That(sut.Position, Is.EqualTo(11));
            Assert.That(sut.ReadLine(), Is.Null);
        }
    }
}
