using System;
using System.Runtime.Serialization;

namespace Commons
{
    [DataContract]
    public struct UnitValue : IComparable
    {
        public UnitValue(Unit unit, double value) : this()
        {
            Unit = unit;
            Value = value;
        }

        [DataMember]
        public double Value { get; private set; }
        [DataMember]
        public Unit Unit { get; private set; }

        public static bool operator <(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.CanConvertTo(value2.Unit))
                throw new InvalidOperationException($"Cannot compare unit values with incompatible units {value1.Unit} and {value2.Unit}");

            return value1.Value < value2.ConvertTo(value1.Unit).Value;
        }
        public static bool operator >(UnitValue value1, UnitValue value2)
        {
            return value2 < value1;
        }
        public static bool operator <=(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.CanConvertTo(value2.Unit))
                throw new InvalidOperationException($"Cannot compare unit values with incompatible units {value1.Unit} and {value2.Unit}");

            return value1.Value <= value2.ConvertTo(value1.Unit).Value;
        }
        public static bool operator >=(UnitValue value1, UnitValue value2)
        {
            return value2 <= value1;
        }
        public static bool operator ==(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.CanConvertTo(value2.Unit))
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
            if (!value1.Unit.CanConvertTo(value2.Unit))
                throw new InvalidOperationException($"Cannot sum unit values with incompatible units {value1.Unit} and {value2.Unit}");

            var value2InValue1Units = value2.ConvertTo(value1.Unit);
            return new UnitValue(value1.Unit, value1.Value + value2InValue1Units.Value);
        }
        public static UnitValue operator -(UnitValue value1, UnitValue value2)
        {
            if (!value1.Unit.CanConvertTo(value2.Unit))
                throw new InvalidOperationException($"Cannot subtract unit values with incompatible units {value1.Unit} and {value2.Unit}");

            var value2InValue1Units = value2.ConvertTo(value1.Unit);
            return new UnitValue(value1.Unit, value1.Value - value2InValue1Units.Value);
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
            var standardValue1 = value1.ConvertToSIStandard();
            var standardValue2 = value2.ConvertToSIStandard();

            switch(standardValue1.Unit)
            {
                case Unit.Meter:
                    break;
                case Unit.MetersPerSecond:
                    if(standardValue2.Unit == Unit.Second)
                        return new UnitValue(Unit.Meter, standardValue1.Value * standardValue2.Value);
                    break;
                case Unit.MetersPerSecondSquared:
                    if (standardValue2.Unit == Unit.Second)
                        return new UnitValue(Unit.MetersPerSecond, standardValue1.Value * standardValue2.Value);
                    break;
                case Unit.Second:
                    if (standardValue2.Unit == Unit.MetersPerSecond)
                        return new UnitValue(Unit.Meter, standardValue1.Value * standardValue2.Value);
                    if (standardValue2.Unit == Unit.MetersPerSecondSquared)
                        return new UnitValue(Unit.MetersPerSecond, standardValue1.Value * standardValue2.Value);
                    break;
            }
            throw new NotSupportedException($"Multiplication of units {value1.Unit} and {value2.Unit} is not supported");
        }
        public static UnitValue operator /(UnitValue value1, UnitValue value2)
        {
            var standardValue1 = value1.ConvertToSIStandard();
            var standardValue2 = value2.ConvertToSIStandard();

            switch (standardValue1.Unit)
            {
                case Unit.Meter:
                    if (standardValue2.Unit == Unit.Second)
                        return new UnitValue(Unit.MetersPerSecond, standardValue1.Value / standardValue2.Value);
                    if (standardValue2.Unit == Unit.MetersPerSecond)
                        return new UnitValue(Unit.Second, standardValue1.Value / standardValue2.Value);
                    break;
                case Unit.MetersPerSecond:
                    if (standardValue2.Unit == Unit.Second)
                        return new UnitValue(Unit.MetersPerSecondSquared, standardValue1.Value / standardValue2.Value);
                    break;
                case Unit.MetersPerSecondSquared:
                    break;
                case Unit.Second:
                    break;
            }
            throw new NotSupportedException($"Division of units {value1.Unit} and {value2.Unit} is not supported");
        }

        public override bool Equals(object other)
        {
            return Value == (other as UnitValue?)?.ConvertTo(Unit).Value;
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
                return this.ConvertToSIStandard().Value.CompareTo(otherUnitValue.ConvertToSIStandard().Value);
            }
            return 0;
        }

        public UnitValue Clone()
        {
            return new UnitValue(Unit, Value);
        }

        public override string ToString()
        {
            return Value + " " + Unit.StringRepresentation();
        }
    }

    public enum Unit
    {
        // Distances
        Meter,
        Feet,
        StatuteMile,
        NauticalMile,

        // Velocities
        MetersPerSecond,
        FeetPerMinute,
        Knots,
        Mach,

        // Acceleration
        MetersPerSecondSquared,
        KnotsPerSeond,

        // Time
        Second,

        // Temperature
        Kelvin,
        Celcius,
        Fahrenheit,

        // Pressure
        Pascal,
        Bar,
        InchesOfMercury,

        // Area
        SquareMeter,

        // Volume
        CubicMeters,

        // Mass
        Kilogram,
        GramPerMol,

        // Charge,
        Coulombs,
        ElementaryCharge,

        // Energy,
        Joule,
        ElectronVolts,
    }
}
