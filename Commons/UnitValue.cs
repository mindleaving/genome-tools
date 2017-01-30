using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Commons
{
    [DataContract]
    public struct UnitValue : IComparable
    {
        public UnitValue(Unit unit, double value) : this()
        {
            Unit = unit.ToCompoundUnit();
            var conversionResult = value.ConvertToSI(unit);
            Value = conversionResult.Value;
        }
        public UnitValue(CompoundUnit unit, double value) : this()
        {
            Unit = unit;
            Value = value;
        }

        [DataMember]
        public double Value { get; private set; }
        [DataMember]
        public CompoundUnit Unit { get; private set; }

        public static bool operator <(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.Equals(value2.Unit))
                throw new InvalidOperationException($"Cannot compare unit values with incompatible units {value1.Unit} and {value2.Unit}");

            return value1.Value < value2.Value;
        }
        public static bool operator >(UnitValue value1, UnitValue value2)
        {
            return value2 < value1;
        }
        public static bool operator <=(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.Equals(value2.Unit))
                throw new InvalidOperationException($"Cannot compare unit values with incompatible units {value1.Unit} and {value2.Unit}");

            return value1.Value <= value2.Value;
        }
        public static bool operator >=(UnitValue value1, UnitValue value2)
        {
            return value2 <= value1;
        }
        public static bool operator ==(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.Equals(value2.Unit))
                throw new InvalidOperationException($"Cannot compare unit values with incompatible units {value1.Unit} and {value2.Unit}");

            return value1.Equals(value2);
        }
        public static bool operator !=(UnitValue value1, UnitValue value2)
        {
            return !(value1 == value2);
        }
        public static UnitValue operator -(UnitValue value1)
        {
            return new UnitValue(value1.Unit, -value1.Value);
        }
        public static UnitValue operator +(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.Equals(value2.Unit))
                throw new InvalidOperationException($"Cannot sum unit values with incompatible units {value1.Unit} and {value2.Unit}");

            return (value1.Value + value2.Value).To(value1.Unit);
        }
        public static UnitValue operator -(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.Equals(value2.Unit))
                throw new InvalidOperationException($"Cannot subtract unit values with incompatible units {value1.Unit} and {value2.Unit}");

            return (value1.Value - value2.Value).To(value1.Unit);
        }
        public static UnitValue operator *(double scalar, UnitValue unitValue)
        {
            return new UnitValue(unitValue.Unit, scalar * unitValue.Value);
        }
        public static UnitValue operator *(int scalar, UnitValue unitValue)
        {
            return new UnitValue(unitValue.Unit, scalar * unitValue.Value);
        }
        public static UnitValue operator /(UnitValue unitValue, double scalar)
        {
            return new UnitValue(unitValue.Unit, unitValue.Value / scalar);
        }
        public static UnitValue operator /(UnitValue unitValue, int scalar)
        {
            return new UnitValue(unitValue.Unit, unitValue.Value / scalar);
        }
        public static UnitValue operator *(UnitValue value1, UnitValue value2)
        {
            return (value1.Value*value2.Value).To(value1.Unit*value2.Unit);
        }
        public static UnitValue operator /(UnitValue value1, UnitValue value2)
        {
            return (value1.Value / value2.Value).To(value1.Unit / value2.Unit);
        }

        public override bool Equals(object other)
        {
            var otherUnitValue = other as UnitValue?;
            return Value == otherUnitValue?.Value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is UnitValue)
            {
                var otherUnitValue = (UnitValue) obj;
                return Value.CompareTo(otherUnitValue.Value);
            }
            return 0;
        }

        public UnitValue Clone()
        {
            return new UnitValue(Unit, Value);
        }

        public override string ToString()
        {
            var unit = Unit.ToUnit();
            var appropriateSIPrefix = SelectSIPrefix(Value);
            var multiplier = appropriateSIPrefix.GetMultiplier();
            var valueString = (Value/multiplier).ToString("F2", CultureInfo.InvariantCulture) 
                + " "
                + appropriateSIPrefix.StringRepresentation();
            if (unit == Commons.Unit.Compound)
                return valueString + Unit;
            return valueString + unit.StringRepresentation();
        }

        private static SIPrefix SelectSIPrefix(double value)
        {
            var allPrefixes = ((SIPrefix[]) Enum.GetValues(typeof(SIPrefix)))
                .ToDictionary(x => x, UnitValueExtensions.GetMultiplier);
            var multipliersSmallerThanValue = allPrefixes.Where(kvp => kvp.Value < value).ToList();
            if (!multipliersSmallerThanValue.Any())
                return allPrefixes.MinimumItem(kvp => kvp.Value).Key;
            return multipliersSmallerThanValue.MaximumItem(kvp => kvp.Value).Key;
        }
    }
}
