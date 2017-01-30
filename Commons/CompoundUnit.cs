using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Commons
{
    [DataContract]
    public class CompoundUnit : IEquatable<CompoundUnit>
    {
        public CompoundUnit() { }
        public CompoundUnit(IEnumerable<SIBaseUnit> nominatorUnits)
            : this(nominatorUnits, Enumerable.Empty<SIBaseUnit>())
        { }
        public CompoundUnit(IEnumerable<SIBaseUnit> nominatorUnits, IEnumerable<SIBaseUnit> denominatorUnits)
        {
            foreach (var nominatorUnit in nominatorUnits)
            {
                if (!UnitExponents.ContainsKey(nominatorUnit))
                    UnitExponents.Add(nominatorUnit, 1);
                else
                    UnitExponents[nominatorUnit]++;
            }
            foreach (var denominatorUnit in denominatorUnits)
            {
                if (!UnitExponents.ContainsKey(denominatorUnit))
                    UnitExponents.Add(denominatorUnit, -1);
                else
                    UnitExponents[denominatorUnit]--;
            }
        }

        [DataMember]
        public Dictionary<SIBaseUnit, int> UnitExponents { get; private set; } = new Dictionary<SIBaseUnit, int>();

        public static CompoundUnit operator *(CompoundUnit unit1, CompoundUnit unit2)
        {
            var multipliedUnit = new CompoundUnit();

            unit1.UnitExponents.ForEach(kvp =>
            {
                if (!multipliedUnit.UnitExponents.ContainsKey(kvp.Key))
                    multipliedUnit.UnitExponents.Add(kvp.Key, kvp.Value);
                else
                    multipliedUnit.UnitExponents[kvp.Key] += kvp.Value;
            });
            unit2.UnitExponents.ForEach(kvp =>
            {
                if (!multipliedUnit.UnitExponents.ContainsKey(kvp.Key))
                    multipliedUnit.UnitExponents.Add(kvp.Key, kvp.Value);
                else
                    multipliedUnit.UnitExponents[kvp.Key] += kvp.Value;
            });

            return multipliedUnit;
        }

        public static CompoundUnit operator /(CompoundUnit unit1, CompoundUnit unit2)
        {
            var divisionUnit = new CompoundUnit();

            unit1.UnitExponents.ForEach(kvp =>
            {
                if (!divisionUnit.UnitExponents.ContainsKey(kvp.Key))
                    divisionUnit.UnitExponents.Add(kvp.Key, kvp.Value);
                else
                    divisionUnit.UnitExponents[kvp.Key] += kvp.Value;
            });
            unit2.UnitExponents.ForEach(kvp =>
            {
                if (!divisionUnit.UnitExponents.ContainsKey(kvp.Key))
                    divisionUnit.UnitExponents.Add(kvp.Key, -kvp.Value);
                else
                    divisionUnit.UnitExponents[kvp.Key] -= kvp.Value;
            });

            return divisionUnit;
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

        public int[] GetUnitFingerprint()
        {
            var baseUnits = (SIBaseUnit[]) Enum.GetValues(typeof(SIBaseUnit));
            var fingerprint = new int[baseUnits.Length];
            for (int unitIdx = 0; unitIdx < baseUnits.Length; unitIdx++)
            {
                var siBaseUnit = baseUnits[unitIdx];
                fingerprint[unitIdx] = UnitExponents.ContainsKey(siBaseUnit) ? UnitExponents[siBaseUnit] : 0;
            }
            return fingerprint;
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
                .Where(kvp => kvp.Value > 0);
            var denominator = UnitExponents
                .Where(kvp => kvp.Value < 0)
                .ToList();
            var unitString = "";
            foreach (var unitMultiplicity in nominator)
            {
                unitString += UnitValueExtensions.StringRepresentation(unitMultiplicity.Key);
                if (unitMultiplicity.Value > 1)
                    unitString += "^" + unitMultiplicity.Value;
                unitString += " ";
            }
            if (denominator.Any())
                unitString += "/";
            if (denominator.Count > 1)
                unitString += "(";
            foreach (var unitMultiplicity in denominator)
            {
                unitString += UnitValueExtensions.StringRepresentation(unitMultiplicity.Key);
                if (-unitMultiplicity.Value > 1)
                    unitString += "^" + -unitMultiplicity.Value;
                unitString += " ";
            }
            if (denominator.Count > 1)
                unitString += ")";
            return unitString;
        }
    }
}