using System;

namespace Commons
{
    public static class UnitConverter
    {
        public static CompoundUnit ToCompoundUnit(this Unit unit)
        {
            switch (unit.ToSIUnit())
            {
                case Unit.Meter:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Meter });
                case Unit.MetersPerSecond:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Meter },
                        new[] { SIBaseUnit.Second });
                case Unit.MetersPerSecondSquared:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Meter },
                        new[] { SIBaseUnit.Second, SIBaseUnit.Second });
                case Unit.Second:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Second });
                case Unit.Kelvin:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Kelvin });
                case Unit.Pascal:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Kilogram },
                        new[] { SIBaseUnit.Meter, SIBaseUnit.Second, SIBaseUnit.Second });
                case Unit.SquareMeter:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Meter, SIBaseUnit.Meter });
                case Unit.CubicMeters:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Meter, SIBaseUnit.Meter, SIBaseUnit.Meter });
                case Unit.Kilogram:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Kilogram });
                case Unit.GramPerMole:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Kilogram },
                        new[] { SIBaseUnit.Mole });
                case Unit.Coulombs:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Ampere, SIBaseUnit.Second });
                case Unit.Joule:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Kilogram, SIBaseUnit.Meter, SIBaseUnit.Meter },
                        new[] { SIBaseUnit.Second, SIBaseUnit.Second });
                case Unit.Newton:
                    return new CompoundUnit(
                        new[] { SIBaseUnit.Kilogram, SIBaseUnit.Meter },
                        new[] { SIBaseUnit.Second, SIBaseUnit.Second });
                default:
                    throw new NotImplementedException();
            }
        }

        public static bool IsSIUnit(this Unit unit)
        {
            return unit.InSet(
                Unit.Meter,
                Unit.MetersPerSecond,
                Unit.MetersPerSecondSquared,
                Unit.Second,
                Unit.Kelvin,
                Unit.Pascal,
                Unit.SquareMeter,
                Unit.CubicMeters,
                Unit.Kilogram,
                Unit.GramPerMole,
                Unit.Coulombs,
                Unit.Joule,
                Unit.Newton);
        }

        public static Unit ToSIUnit(this Unit unit)
        {
            if (unit.IsSIUnit())
                return unit;

            switch (unit)
            {
                case Unit.Feet:
                case Unit.StatuteMile:
                case Unit.NauticalMile:
                    return Unit.Meter;
                case Unit.FeetPerMinute:
                case Unit.Knots:
                case Unit.Mach:
                    return Unit.MetersPerSecond;
                case Unit.KnotsPerSeond:
                    return Unit.MetersPerSecondSquared;
                case Unit.Celcius:
                case Unit.Fahrenheit:
                    return Unit.Kelvin;
                case Unit.Bar:
                case Unit.InchesOfMercury:
                    return Unit.Pascal;
                case Unit.ElementaryCharge:
                    return Unit.Coulombs;
                case Unit.ElectronVolts:
                    return Unit.Joule;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
            }
        }

        public static UnitConversionResult ConvertToSI(this double value, Unit unit)
        {
            switch (unit)
            {
                case Unit.Compound:
                    throw new NotSupportedException("Cannot convert compound unit");
                case Unit.Feet:
                    return new UnitConversionResult(Unit.Meter, value * 0.3048);
                case Unit.NauticalMile:
                    return new UnitConversionResult(Unit.Meter, value * 1852);
                case Unit.StatuteMile:
                    return new UnitConversionResult(Unit.Meter, value * 1609.344);
                case Unit.FeetPerMinute:
                    return new UnitConversionResult(Unit.MetersPerSecond, value * 0.00508);
                case Unit.Knots:
                    return new UnitConversionResult(Unit.MetersPerSecond, value * 0.514444444);
                case Unit.Mach:
                    return new UnitConversionResult(Unit.MetersPerSecond, value * 340.29);
                case Unit.KnotsPerSeond:
                    return new UnitConversionResult(Unit.MetersPerSecondSquared, value * 0.514444444);
                case Unit.Celcius:
                    return new UnitConversionResult(Unit.Kelvin, value - 273.15);
                case Unit.Fahrenheit:
                    return new UnitConversionResult(Unit.Kelvin, (value + 459.67) * (5.0 / 9.0));
                case Unit.Bar:
                    return new UnitConversionResult(Unit.Pascal, value * 1e5);
                case Unit.InchesOfMercury:
                    return new UnitConversionResult(Unit.Pascal, value * 3386.38816);
                case Unit.ElementaryCharge:
                    return new UnitConversionResult(Unit.Coulombs, value * 1.60217662 * 1e-19);
                case Unit.ElectronVolts:
                    return new UnitConversionResult(Unit.Joule, value * 1.60217662 * 1e-19);
                case Unit.Meter:
                case Unit.MetersPerSecond:
                case Unit.MetersPerSecondSquared:
                case Unit.Second:
                case Unit.Kelvin:
                case Unit.Pascal:
                case Unit.SquareMeter:
                case Unit.CubicMeters:
                case Unit.Kilogram:
                case Unit.GramPerMole:
                case Unit.Coulombs:
                case Unit.Joule:
                case Unit.Newton:
                    return new UnitConversionResult(unit, value);
                default:
                    throw new NotSupportedException($"Conversion of {unit} to standard is not implemented");
            }
        }
    }

    public class UnitConversionResult
    {
        public UnitConversionResult(Unit unit, double value)
        {
            Unit = unit;
            Value = value;
        }

        public Unit Unit { get; }
        public double Value { get; }
    }
}
