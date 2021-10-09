using System;
using System.Linq;
using GenomeTools.ChemistryLibrary.Extensions;
using NUnit.Framework;

namespace GenomeTools.ChemistryLibraryTest.Extensions
{
    public class EnumerableExtensionsTest
    {
        [Test]
        public void SplitReturnsNoItemsForEmptyInput()
        {
            var input = Array.Empty<byte>();
            var actual = input.Split<byte>(0x0).ToList();
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void SplitReturnsSingleItem()
        {
            var input = new byte[] { 0x1 };
            var actual = input.Split<byte>(0x0).ToList();
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0], Is.EqualTo(new[] { 0x1 }));
        }

        [Test]
        public void SplitReturnsSingleItemAfterSeparator()
        {
            var input = new byte[] { 0x0, 0x1 };
            var actual = input.Split<byte>(0x0).ToList();
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[0], Is.Empty);
            Assert.That(actual[1], Is.EqualTo(new[] { 0x1 }));
        }

        [Test]
        public void SplitReturnsTwoEmptyItemsIfOnlySeparator()
        {
            var input = new byte[] { 0x0 };
            var actual = input.Split<byte>(0x0).ToList();
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[0], Is.Empty);
            Assert.That(actual[1], Is.Empty);
        }

        [Test]
        public void SplitReturnsExpectedItems()
        {
            var input = new[] { 0x1, 0x2, 0x0, 0x3, 0x4, 0x5, 0x0 };
            var actual = input.Split(0x0).ToList();
            Assert.That(actual.Count, Is.EqualTo(3));
            Assert.That(actual[0], Is.EqualTo(new[] { 0x1, 0x2 }));
            Assert.That(actual[1], Is.EqualTo(new[] { 0x3, 0x4, 0x5 }));
            Assert.That(actual[2], Is.Empty);
        }

        [Test]
        public void SplitRemovesEmptyEntries()
        {
            var input = new[] { 0x1, 0x2, 0x0, 0x0, 0x3, 0x4, 0x5, 0x0 };
            var actual = input.Split(0x0, StringSplitOptions.RemoveEmptyEntries).ToList();
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[0], Is.EqualTo(new[] { 0x1, 0x2 }));
            Assert.That(actual[1], Is.EqualTo(new[] { 0x3, 0x4, 0x5 }));
        }
    }
}
