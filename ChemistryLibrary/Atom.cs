using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public class Atom
    {
        public int Protons { get; }
        public int Neutrons { get; }
        public int Period { get; }
        public ElementName Element { get; }
        public UnitValue Mass { get; }
        public List<Orbital> Orbitals { get; }
        public IEnumerable<Electron> Electrons => Orbitals.SelectMany(orbital => orbital.Electrons);
        public bool IsExcitated { get { throw new NotImplementedException();} }

        public Atom(int protons, int neutrons)
        {
            Protons = protons;
            Neutrons = neutrons;
            Element = ElementMap.FromProtonCount(Protons);
            Period = PeriodicTable.GetPeriod(Element);
            Mass = AtomPropertyCalculator.CalculateMass(Protons, Neutrons);
            Orbitals = OrbitalGenerator.Generate(this, Period+1);

            PopulateOrbitalsInGroundState();
        }

        private void PopulateOrbitalsInGroundState()
        {
            var energySortedOrbitals = Orbitals.OrderBy(x => x, OrbitalComparer.Instance).ToList();
            if(energySortedOrbitals.Any(orbtial => orbtial.Electrons.Any()))
                throw new InvalidOperationException("Population of orbits only implemented for all orbits being empty");
            var currentOrbitalidx = 0;
            var currentOrbital = energySortedOrbitals[currentOrbitalidx];
            for (int electronIdx = 0; electronIdx < Protons; electronIdx++)
            {
                if (currentOrbital.IsFull)
                {
                    currentOrbitalidx++;
                    currentOrbital = energySortedOrbitals[currentOrbitalidx];
                }
                var electron = new Electron();
                currentOrbital.AddElectron(electron);
            }
        }

        /// <summary>
        /// Make energy available to atom for excitation. May not be excepted 
        /// e.g. if energy not enough for reaching next energy state
        /// </summary>
        /// <param name="ingressEnergy">Incoming energy</param>
        /// <param name="unconsumedEnergy">Energy not consumed by excitation</param>
        /// <returns>True if some of the energy was accepted for excitation. </returns>
        public bool TryExcitateAtom(UnitValue ingressEnergy, out UnitValue unconsumedEnergy)
        {
            throw new NotImplementedException();
        }
    }

    public class OrbitalComparer : IComparer<Orbital>
    {
        public static OrbitalComparer Instance { get; } = new OrbitalComparer();

        public int Compare(Orbital orbital1, Orbital orbital2)
        {
            var orbtial1Order = CalculateOrbitalOrder(orbital1);
            var orbital2Order = CalculateOrbitalOrder(orbital2);
            return orbtial1Order.CompareTo(orbital2Order);
        }

        private static int CalculateOrbitalOrder(Orbital orbital1)
        {
            return (orbital1.Period - 1) * orbital1.Period / 2 + (int)orbital1.Type;
        }
    }

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
