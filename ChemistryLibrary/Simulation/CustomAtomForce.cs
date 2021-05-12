using System;
using Commons.Mathematics;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation
{
    public class CustomAtomForce
    {
        public uint AtomVertex { get; set; }
        public Func<Atom, UnitValue, Vector3D> ForceFunc { get; set; }
    }
}
