using System;

namespace ChemistryLibrary.Objects
{
    public class ChemistryException : Exception
    {
        public ChemistryException() : base()
        {
        }
        public ChemistryException(string explanation) : base(explanation)
        {
        }
    }
}