using System.Linq;
using GenomeTools.ChemistryLibrary.IO.Sam;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Sam
{
    public class NaturalStringComparerTest
    {
        [Test]
        public void ValuesInExpectedOrder()
        {
            var expected = new[]
            {
                null, "abc", "abc+5", "abc-5", "abc.d", "abc03", "abc5", "abc008", "abc08", "abc8", "abc17", "abc17.+", "abc17.2", "abc17.d", "abc59", "abcd"
            };
            var input = expected.Reverse();
            var sut = new NaturalStringComparer();

            var actual = input.OrderBy(x => x, sut);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
