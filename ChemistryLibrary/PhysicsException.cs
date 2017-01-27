using System;

namespace ChemistryLibrary
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