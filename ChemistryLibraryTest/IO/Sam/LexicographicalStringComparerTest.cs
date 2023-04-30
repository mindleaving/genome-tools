using System.Linq;
using GenomeTools.ChemistryLibrary.IO.Sam;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.IO.Sam
{
    public class LexicographicalStringComparerTest
    {
        [Test]
        public void ValuesInExpectedOrder()
        {
            var expected = new[] { null, "abc", "abc17", "abc5", "abc59", "abcd" };
            var input = expected.Reverse();
            var sut = new LexicographicalStringComparer();

            var actual = input.OrderBy(x => x, sut);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
