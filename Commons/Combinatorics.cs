using System.Collections.Generic;
using System.Linq;

namespace Commons
{
    public static class Combinatorics
    {
        public static IEnumerable<IEnumerable<T>> GenerateCombination<T>(ICollection<T> letters, int level)
        {
            foreach (var letter in letters)
            {
                if (level == 1)
                    yield return new []{letter};
                else
                {
                    foreach (var subSequence in GenerateCombination(letters, level - 1))
                    {
                        yield return subSequence.Concat(new[] { letter });
                    }
                }
            }
        }
    }
}