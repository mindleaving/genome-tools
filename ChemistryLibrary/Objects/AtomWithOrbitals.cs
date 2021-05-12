﻿using System;
using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.DataLookups;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class AtomWithOrbitals : Atom
    {
        public List<Orbital> Orbitals { get; }
        public IEnumerable<Electron> Electrons => Orbitals.SelectMany(orbital => orbital.Electrons);
        public IEnumerable<Electron> ValenceElectrons => OuterOrbitals.SelectMany(o => o.Electrons);
        public IEnumerable<Orbital> OuterOrbitals => Orbitals.Where(o => o.Period == Period);
        public IEnumerable<Orbital> LonePairs => OuterOrbitals.Where(o => o.IsFull && !o.IsPartOfBond);
        public IEnumerable<Orbital> OrbitalsAvailableForBonding => OuterOrbitals.Where(o => !o.IsFull && !o.IsEmpty);
        public bool IsExcitated
        {
            get
            {
                var highestOccupiedEnergy = Orbitals.OrderBy(o => o.Energy).Last(o => o.Electrons.Any()).Energy;
                return Orbitals.Any(o => o.Energy < highestOccupiedEnergy && !o.IsFull);
            }
        }

        public AtomWithOrbitals(int protons, int neutrons)
            : base(protons, neutrons)
        {
            Orbitals = OrbitalGenerator.Generate(this, Period + 1);
            FormalCharge = (Protons - Electrons.Count()) * PhysicalConstants.ElementaryCharge;
            EffectiveCharge = FormalCharge;

            PopulateOrbitalsInGroundState();
            ExcitateForMaximalBondAvailability();
        }

        //public static AtomWithOrbitals FromStableIsotope(ElementName element)
        //{
        //    var isotope = IsotopeTable.GetStableIsotopeOf(element).FirstOrDefault();
        //    if (isotope == null)
        //        throw new ChemistryException($"No stable isotope known for {element}");
        //    return new AtomWithOrbitals(isotope.Protons, isotope.Neutrons);
        //}

        private void PopulateOrbitalsInGroundState()
        {
            if (Orbitals.Any(o => !o.IsEmpty))
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
                    if (electronCount == Protons)
                        break;
                }
                if (electronCount == Protons)
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
        /// Excitates outer orbitals, to achieve as many 
        /// s- and p-oribtals with one electron
        /// </summary>
        private void ExcitateForMaximalBondAvailability()
        {
            var emptyOuterSorPOribtals = new Queue<Orbital>(OuterOrbitals
                .Where(o => o.Type.InSet(OrbitalType.s, OrbitalType.p) && o.IsEmpty));
            var fullOuterOrbitals = new Queue<Orbital>(OuterOrbitals.Where(o => o.IsFull));
            while (emptyOuterSorPOribtals.Any() && fullOuterOrbitals.Any())
            {
                var fullOrbital = fullOuterOrbitals.Dequeue();
                var electron = fullOrbital.RemoveElectron();
                var emptyOrbital = emptyOuterSorPOribtals.Dequeue();
                emptyOrbital.AddElectron(electron);
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

        public override string ToString()
        {
            return Element.ToString();
        }
    }
}
