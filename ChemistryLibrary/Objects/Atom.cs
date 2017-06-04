using System;
using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.DataLookups;
using ChemistryLibrary.Measurements;
using Commons;

namespace ChemistryLibrary.Objects
{
    public class Atom
    {
        private UnitPoint3D position;

        public string Id { get; } = Guid.NewGuid().ToString();

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
        /// <summary>
        /// Position in units of picometer
        /// </summary>
        public UnitPoint3D Position
        {
            get { return position; }
            set
            {
                if(position != null && IsPositionFixed)
                    throw new InvalidOperationException("Fixed atoms must not be moved");
                position = value;
            }
        }
        /// <summary>
        /// Velocity in meters per second
        /// </summary>
        public UnitVector3D Velocity { get; set; } = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);
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
        /// <summary>
        /// Marks atom as part of the backbone, e.g. of a peptide chain
        /// </summary>
        public bool IsBackbone { get; set; }
        /// <summary>
        /// IUPAC nomenclature atom name for atoms in amino acids,
        /// e.g. N, CA, CB, CG, etc.
        /// </summary>
        public string AminoAcidAtomName { get; set; }
        /// <summary>
        /// True if atom is fixed to its current position.
        /// </summary>
        public bool IsPositionFixed { get; set; }
        /// <summary>
        /// Helper flag for <see cref="MoleculePositioner"/>.
        /// </summary>
        public bool IsPositioned { get; set; }

        public Atom(int protons, int neutrons)
        {
            Protons = protons;
            Neutrons = neutrons;
            Element = ElementMap.FromProtonCount(Protons);
            Period = PeriodicTable.GetPeriod(Element);
            Mass = AtomPropertyCalculator.CalculateMass(Protons, Neutrons);
            Radius = PeriodicTable.GetRadius(Element);
            Orbitals = OrbitalGenerator.Generate(this, Period+1);
            ElectroNegativity = PeriodicTable.ElectroNegativity(Element);

            PopulateOrbitalsInGroundState();
            ExcitateForMaximalBondAvailability();

            FormalCharge = (Protons - Electrons.Count()) * PhysicalConstants.ElementaryCharge;
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            if (!(obj is Atom))
                return false;
            return Id == ((Atom)obj).Id;
        }

        public override string ToString()
        {
            return Element.ToString();
        }
    }
}
