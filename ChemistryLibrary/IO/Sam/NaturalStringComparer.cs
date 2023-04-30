using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            var xIndex = 0;
            var yIndex = 0;
            while (xIndex < x.Length && yIndex < y.Length)
            {
                var xChar = x[xIndex];
                var yChar = y[yIndex];
                if (char.IsDigit(xChar) && char.IsDigit(yChar))
                {
                    var xBuffer = new string(x.Skip(xIndex).TakeWhile(char.IsDigit).ToArray());
                    var yBuffer = new string(y.Skip(yIndex).TakeWhile(char.IsDigit).ToArray());
                    var xInteger = int.Parse(xBuffer);
                    var yInteger = int.Parse(yBuffer);
                    var comparisonResult = xInteger.CompareTo(yInteger);
                    if (comparisonResult != 0)
                        return comparisonResult;
                    comparisonResult = xBuffer.Length.CompareTo(yBuffer.Length);
                    if (comparisonResult != 0)
                        return -comparisonResult;
                    xIndex += xBuffer.Length;
                    yIndex += yBuffer.Length;
                }
                else
                {
                    var comparisonResult = xChar.CompareTo(yChar);
                    if (comparisonResult != 0)
                        return comparisonResult;
                    xIndex++;
                    yIndex++;
                }
            }
            return x.Length.CompareTo(y.Length);
        }
    }
}