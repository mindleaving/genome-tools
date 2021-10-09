using System;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IReadOnlyCollection<T> array, T separator, StringSplitOptions options = StringSplitOptions.None)
        {
            if(array.Count == 0)
                yield break;
            var index = 0;
            while (options.HasFlag(StringSplitOptions.RemoveEmptyEntries) ? array.Count > index : array.Count >= index)
            {
                var subArray = array.Skip(index).TakeWhile(x => !x.Equals(separator)).ToArray();
                if(!options.HasFlag(StringSplitOptions.RemoveEmptyEntries) || subArray.Length > 0)
                    yield return subArray;
                index += subArray.Length + 1; // +1 for separator
            }
        }
    }
}
