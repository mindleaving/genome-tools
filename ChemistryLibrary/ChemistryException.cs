using System;

namespace ChemistryLibrary
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