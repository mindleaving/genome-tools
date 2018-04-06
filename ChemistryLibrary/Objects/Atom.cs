using System;
using System.Linq;
using ChemistryLibrary.Builders;
using ChemistryLibrary.DataLookups;
using ChemistryLibrary.Measurements;
using Commons.Physics;

namespace ChemistryLibrary.Objects
{
    public class Atom : IEquatable<Atom>
    {
        private string Id { get; } = Guid.NewGuid().ToString();


        public Atom(int protons, int neutrons)
        {
            Protons = protons;
            Neutrons = neutrons;
            Element = ElementMap.FromProtonCount(Protons);
            Period = PeriodicTable.GetPeriod(Element);
            Mass = AtomPropertyCalculator.CalculateMass(Protons, Neutrons);
            Radius = PeriodicTable.GetRadius(Element);
            ElectroNegativity = PeriodicTable.ElectroNegativity(Element);
            FormalCharge =  0*PhysicalConstants.ElementaryCharge;
            EffectiveCharge = FormalCharge;
        }

        public static Atom FromStableIsotope(ElementName element)
        {
            var isotope = IsotopeTable.GetStableIsotopeOf(element).FirstOrDefault();
            if (isotope == null)
                throw new ChemistryException($"No stable isotope known for {element}");
            return new Atom(isotope.Protons, isotope.Neutrons);
        }

        public int Protons { get; }
        public int Neutrons { get; }
        public int Period { get; }
        public ElementName Element { get; }
        public UnitValue Mass { get; }
        public UnitValue Radius { get; }
        public double ElectroNegativity { get; }
        public UnitValue FormalCharge { get; protected set; }
        public UnitValue EffectiveCharge { get; set; }

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

        private UnitPoint3D position;
        /// <summary>
        /// Position in units of picometer
        /// </summary>
        public UnitPoint3D Position
        {
            get { return position; }
            set
            {
                if (position != null && IsPositionFixed)
                    throw new InvalidOperationException("Fixed atoms must not be moved");
                position = value;
            }
        }

        /// <summary>
        /// Velocity in meters per second
        /// </summary>
        public UnitVector3D Velocity { get; set; } = new UnitVector3D(Unit.MetersPerSecond, 0, 0, 0);

        public bool Equals(Atom other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Atom) obj);
        }

        public override int GetHashCode()
        {
            return Id != null ? Id.GetHashCode() : 0;
        }
    }
}
