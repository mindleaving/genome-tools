using System;
using System.Linq;

namespace Commons
{
    public static class StringExtensions
    {
        public static string FirstLetterToUpper(this string input)
        {
            var words = input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            return words
                .Select(x => x.Substring(0, 1).ToUpper() + x.Substring(1).ToLower())
                .Aggregate((a, b) => a + " " + b);
        }
    }
}
