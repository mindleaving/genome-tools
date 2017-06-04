using System;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public class CustomAtomForce
    {
        public uint AtomVertex { get; set; }
        public Func<Atom, UnitValue, Vector3D> ForceFunc { get; set; }
    }
}
