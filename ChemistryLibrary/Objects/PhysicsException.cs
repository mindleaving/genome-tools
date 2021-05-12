using System;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class PhysicsException : Exception
    {
        public PhysicsException() : base()
        {
        }
        public PhysicsException(string explanation) : base(explanation)
        {
        }
    }
}