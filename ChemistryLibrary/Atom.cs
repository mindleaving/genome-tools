﻿using System;
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
        public UnitValue Radius { get; }
        public double ElectroNegativity { get; }
        public UnitValue FormalCharge { get; }
        public UnitValue EffectiveCharge { get; set; }
        public List<Orbital> Orbitals { get; }
        public UnitPoint3D Position { get; set; }
        public IEnumerable<Electron> Electrons => Orbitals.SelectMany(orbital => orbital.Electrons);
        public IEnumerable<Electron> ValenceElectrons => OuterOrbitals.SelectMany(o => o.Electrons);
        public IEnumerable<Orbital> OuterOrbitals => Orbitals.Where(o => o.Period == Period);
        public IEnumerable<Orbital> OrbitalsAvailableForBonding => OuterOrbitals.Where(o => !o.IsFull && !o.IsEmpty);
        public bool IsExcitated
        {
            get
            {
                var highestOccupiedEnergy = Orbitals.OrderBy(o => o.Energy).Last(o => o.Electrons.Any()).Energy;
                return Orbitals.Any(o => o.Energy < highestOccupiedEnergy && !o.IsFull);
            }
        }

        public Atom(int protons, int neutrons)
        {
            Protons = protons;
            Neutrons = neutrons;
            Element = ElementMap.FromProtonCount(Protons);
            Period = PeriodicTable.GetPeriod(Element);
            Mass = AtomPropertyCalculator.CalculateMass(Protons, Neutrons);
            Radius = PeriodicTable.GetRadius(Element);
            Orbitals = OrbitalGenerator.Generate(this, Period+1);
            FormalCharge = (Protons - Electrons.Count()) * PhysicalConstants.ElementaryCharge;
            ElectroNegativity = PeriodicTable.ElectroNegativity(Element);

            PopulateOrbitalsInGroundState();
            EffectiveCharge = FormalCharge;
        }

        public static Atom FromStableIsotope(ElementName element)
        {
            var isotope = IsotopeTable.GetStableIsotopeOf(element).FirstOrDefault();
            if(isotope == null)
                throw new ChemistryException($"No stable isotope known for {element}");
            return new Atom(isotope.Protons, isotope.Neutrons);
        }

        private void PopulateOrbitalsInGroundState()
        {
            if(Orbitals.Any(o => !o.IsEmpty))
                throw new InvalidOperationException("Population of orbits only implemented for all orbits being empty");
            var energySortedOrbitals = Orbitals.ToLookup(OrbitalComparer.CalculateOrbitalOrder);
            var energyGroups = energySortedOrbitals.Select(x => x.Key).Distinct().OrderBy(x => x);
            var electronCount = 0;
            foreach (var energyGroup in energyGroups)
            {
                var energyEqualOrbitals = energySortedOrbitals[energyGroup].ToList();
                // Add first electron
                foreach (var energyEqualOrbital in energyEqualOrbitals)
                {
                    var electron = new Electron();
                    energyEqualOrbital.AddElectron(electron);
                    electronCount++;
                    if(electronCount == Protons)
                        break;
                }
                if(electronCount == Protons)
                    break;
                // Add second electron
                foreach (var energyEqualOrbital in energyEqualOrbitals)
                {
                    var electron = new Electron();
                    energyEqualOrbital.AddElectron(electron);
                    electronCount++;
                    if (electronCount == Protons)
                        break;
                }
                if (electronCount == Protons)
                    break;
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
}