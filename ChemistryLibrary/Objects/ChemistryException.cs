using System;

namespace GenomeTools.ChemistryLibrary.Objects
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