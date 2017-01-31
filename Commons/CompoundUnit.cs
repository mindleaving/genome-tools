using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Commons
{
    [DataContract]
    public class CompoundUnit : IEquatable<CompoundUnit>
    {
        public CompoundUnit()
        { }
        public CompoundUnit(IEnumerable<SIBaseUnit> nominatorUnits)
            : this(nominatorUnits, Enumerable.Empty<SIBaseUnit>())
        { }
        public CompoundUnit(IEnumerable<SIBaseUnit> nominatorUnits, IEnumerable<SIBaseUnit> denominatorUnits)
        {
            UnitExponents = new int[siBaseUnits.Count];

            foreach (var nominatorUnit in nominatorUnits)
            {
                var unitIdx = siBaseUnits[nominatorUnit];
                UnitExponents[unitIdx]++;
            }
            foreach (var denominatorUnit in denominatorUnits)
            {
                var unitIdx = siBaseUnits[denominatorUnit];
                UnitExponents[unitIdx]--;
            }
        }
        public CompoundUnit(IEnumerable<int> unitExponents)
        {
            UnitExponents = unitExponents.ToArray();
        }

        [DataMember]
        public int[] UnitExponents { get; private set; }

        public static CompoundUnit operator *(CompoundUnit unit1, CompoundUnit unit2)
        {
            return new CompoundUnit(unit1.UnitExponents.PairwiseOperation(unit2.UnitExponents, (a,b) => a + b));
        }

        public static CompoundUnit operator /(CompoundUnit unit1, CompoundUnit unit2)
        {
            return new CompoundUnit(unit1.UnitExponents.PairwiseOperation(unit2.UnitExponents, (a, b) => a - b));
        }

        public static bool operator ==(CompoundUnit unit1, CompoundUnit unit2)
        {
            return unit1?.Equals(unit2) ?? false;
        }
        public static bool operator ==(CompoundUnit unit1, Unit unit2)
        {
            return unit1?.Equals(unit2.ToCompoundUnit()) ?? false;
        }
        public static bool operator !=(CompoundUnit unit1, CompoundUnit unit2)
        {
            return !(unit1 == unit2);
        }
        public static bool operator !=(CompoundUnit unit1, Unit unit2)
        {
            return !(unit1 == unit2);
        }

        public override bool Equals(object other)
        {
            if (other is Unit)
                return Equals(((Unit) other).ToCompoundUnit());
            return Equals(other as CompoundUnit);
        }

        private static readonly Dictionary<SIBaseUnit, int> siBaseUnits = ((SIBaseUnit[]) Enum.GetValues(typeof(SIBaseUnit)))
            .ToDictionary(unit => unit, unit => (int)unit);
        public int[] GetUnitFingerprint()
        {
            return UnitExponents;
        }

        public bool Equals(CompoundUnit other)
        {
            if ((object)other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return GetUnitFingerprint().SequenceEqual(other.GetUnitFingerprint());
        }

        public override string ToString()
        {
            var nominator = UnitExponents
                .Select((multiplicity, idx) => new
                {
                    SIBaseUnit = (SIBaseUnit)idx,
                    Multiplicity = multiplicity
                })
                .Where(kvp => kvp.Multiplicity > 0);
            var denominator = UnitExponents
                .Select((multiplicity, idx) => new
                {
                    SIBaseUnit = (SIBaseUnit)idx,
                    Multiplicity = multiplicity
                })
                .Where(kvp => kvp.Multiplicity < 0)
                .ToList();
            var unitString = "";
            foreach (var unitMultiplicity in nominator)
            {
                unitString += unitMultiplicity.SIBaseUnit.StringRepresentation();
                if (unitMultiplicity.Multiplicity > 1)
                    unitString += "^" + unitMultiplicity.Multiplicity;
                unitString += " ";
            }
            if (denominator.Any())
                unitString += "/";
            if (denominator.Count > 1)
                unitString += "(";
            foreach (var unitMultiplicity in denominator)
            {
                unitString += unitMultiplicity.SIBaseUnit.StringRepresentation();
                if (-unitMultiplicity.Multiplicity > 1)
                    unitString += "^" + -unitMultiplicity.Multiplicity;
                unitString += " ";
            }
            if (denominator.Count > 1)
                unitString += ")";
            return unitString;
        }
    }
}