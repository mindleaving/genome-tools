using System;
using System.Linq;

namespace Commons
{
    public static class EnumExtensions
    {
        public static bool InSet<T>(this T value, params T[] set) where T: struct, IConvertible
        {
            return set.Contains(value);
        }
    }
}
