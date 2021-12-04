using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.Genomics.Alignment
{
    public class StringProxy : IEnumerable<char>
    {
        private readonly string backingString;

        public StringProxy(string backingString, int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Indices must be non-negative");
            if(startIndex >= backingString.Length || endIndex >= backingString.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Indices must be less than length of backing string");
            if (endIndex < startIndex)
                throw new ArgumentException("End index must be greater than or equal to start index");
            this.backingString = backingString;
            StartIndex = startIndex;
            EndIndex = endIndex;
            Length = EndIndex - StartIndex + 1;
        }

        public int StartIndex { get; }
        public int EndIndex { get; }
        public int Length { get; }

        public char this[int index] => backingString[StartIndex + index];

        public IEnumerator<char> GetEnumerator()
        {
            return backingString.Skip(StartIndex).Take(Length).GetEnumerator();
        }

        public override string ToString()
        {
            return backingString.Substring(StartIndex, Length);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(string searchString)
        {
            return IndexOf(new StringProxy(searchString, 0, searchString.Length - 1));
        }

        public int IndexOf(StringProxy searchString)
        {
            for (int i = StartIndex; i <= EndIndex-searchString.Length+1; i++)
            {
                var isMatch = true;
                for (int matchIndex = 0; matchIndex < searchString.Length; matchIndex++)
                {
                    if (backingString[i + matchIndex] != searchString[matchIndex])
                    {
                        isMatch = false;
                        break;
                    }
                }
                if (isMatch)
                    return i-StartIndex;
            }
            return -1;
        }

        public StringProxy Substring(int startIndex)
        {
            return Substring(startIndex, Length - startIndex);
        }

        public StringProxy Substring(int startIndex, int length)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index must be non-negative");
            if (length == 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero");
            if (startIndex + length > Length)
                throw new ArgumentOutOfRangeException(nameof(length), "Requested length exceeds string proxy length");
            return new StringProxy(backingString, StartIndex + startIndex, StartIndex + startIndex + length - 1);
        }
    }
}