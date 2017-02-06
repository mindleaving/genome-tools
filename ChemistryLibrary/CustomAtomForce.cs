using System;
using Commons;

namespace ChemistryLibrary
{
    public class CustomAtomForce
    {
        public uint AtomVertex { get; set; }
        public Func<Atom, UnitValue, UnitVector3D> ForceFunc { get; set; }
    }
}
