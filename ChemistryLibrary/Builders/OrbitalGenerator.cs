using System.Collections.Generic;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Builders
{
    public static class OrbitalGenerator
    {
        public static List<Orbital> Generate(Atom atom, int maximumPeriod)
        {
            var orbitals = new List<Orbital>();
            for (var period = 1; period <= maximumPeriod; period++)
            {
                // s-orbital
                orbitals.Add(new Orbital(atom, period, OrbitalType.s));
                if(period == 1)
                    continue;

                // p-orbitals
                for (var i = 0; i < 3; i++)
                {
                    orbitals.Add(new Orbital(atom, period, OrbitalType.p));
                }
                if (period == 2)
                    continue;

                // d-orbitals
                for (var i = 0; i < 5; i++)
                {
                    orbitals.Add(new Orbital(atom, period, OrbitalType.d));
                }
                if(period == 3)
                    continue;

                // f-orbitals
                for (int i = 0; i < 7; i++)
                {
                    orbitals.Add(new Orbital(atom, period, OrbitalType.f));
                }
                //if(period == 4)
                //    continue;
            }
            return orbitals;
        }
    }
}