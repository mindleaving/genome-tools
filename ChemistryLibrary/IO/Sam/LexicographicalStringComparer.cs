using System;
using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class LexicographicalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            for (int i = 0; i < Math.Min(x.Length, y.Length); i++)
            {
                var comparisonResult = x[i].CompareTo(y[i]);
                if (comparisonResult != 0)
                    return comparisonResult;
            }
            return x.Length.CompareTo(y.Length);
        }
    }
}