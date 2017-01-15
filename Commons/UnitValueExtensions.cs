using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons
{
    public static class UnitValueExtensions
    {
        private static readonly Unit[] SIStandards = { Unit.Meter, Unit.MetersPerSecond, Unit.Second, Unit.MetersPerSecondSquared, Unit.Kelvin, Unit.Pascal };
        public static UnitValue Abs(this UnitValue unitValue)
        {
            return new UnitValue(unitValue.Unit, Math.Abs(unitValue.Value));
        }
        public static UnitValue ConvertTo(this UnitValue unitValue, Unit newUnit)
        {
            if (!unitValue.Unit.CanConvertTo(newUnit))
                throw new InvalidOperationException($"Cannot convert {unitValue.Unit} to {newUnit}");
            if (unitValue.Unit == newUnit)
                return new UnitValue(unitValue.Unit, unitValue.Value);

            var standardUnitValue = unitValue.ConvertToSIStandard();
            if (standardUnitValue.Unit.Equals(newUnit))
                return standardUnitValue;

            switch (standardUnitValue.Unit)
            {
                case Unit.Meter:
                    if (newUnit == Unit.NauticalMile)
                        return new UnitValue(newUnit, standardUnitValue.Value / 1852.0);
                    if(newUnit == Unit.StatuteMile)
                        return new UnitValue(newUnit, standardUnitValue.Value * 0.000621371);
                    if(newUnit == Unit.Feet)
                        return new UnitValue(newUnit, standardUnitValue.Value * 3.28084);
                    break;
                case Unit.MetersPerSecond:
                    if (newUnit == Unit.Knots)
                        return new UnitValue(newUnit, standardUnitValue.Value * 1.94384449);
                    if (newUnit == Unit.FeetPerMinute)
                        return new UnitValue(newUnit, standardUnitValue.Value * 196.850394);
                    if(newUnit == Unit.Mach)
                        return new UnitValue(newUnit, standardUnitValue.Value/340.3);
                    break;
                case Unit.Second:
                    break;
                case Unit.MetersPerSecondSquared:
                    if (newUnit == Unit.KnotsPerSeond)
                        return new UnitValue(newUnit, standardUnitValue.Value * 1.94384449);
                    break;
                case Unit.Kelvin:
                    if(newUnit == Unit.Celcius)
                        return new UnitValue(newUnit, standardUnitValue.Value - 273.15);
                    if(newUnit == Unit.Fahrenheit)
                        return new UnitValue(newUnit, standardUnitValue.Value*(9.0/5.0)-459.67);
                    break;
                case Unit.Pascal:
                    if(newUnit == Unit.Bar)
                        return new UnitValue(newUnit, standardUnitValue.Value * 0.00001);
                    if (newUnit == Unit.InchesOfMercury)
                        return new UnitValue(newUnit, standardUnitValue.Value * 0.0002952998751);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unitValue.Unit));
            }
            throw new InvalidOperationException($"Conversion from {unitValue.Unit} to {newUnit} is not implemented");
        }
        public static bool CanConvertTo(this UnitValue originalValue, Unit newUnit) { return CanConvertTo(originalValue.Unit, newUnit); }
        public static bool CanConvertTo(this Unit originalUnit, Unit newUnit)
        {
            if (originalUnit.Equals(newUnit))
                return true;
            var originalStandardUnit = originalUnit.CorrespondingSIStandard();
            if (originalStandardUnit.Equals(newUnit))
                return true;
            switch (originalStandardUnit)
            {
                case Unit.Meter:
                    if (newUnit.InSet(Unit.Feet, Unit.NauticalMile, Unit.StatuteMile))
                        return true;
                    break;
                case Unit.MetersPerSecond:
                    if (newUnit.InSet(Unit.Knots, Unit.FeetPerMinute, Unit.Mach))
                        return true;
                    break;
                case Unit.Second:
                    break;
                case Unit.MetersPerSecondSquared:
                    if (newUnit.InSet(Unit.KnotsPerSeond))
                        return true;
                    break;
                case Unit.Kelvin:
                    if (newUnit.InSet(Unit.Celcius, Unit.Fahrenheit))
                        return true;
                    break;
                case Unit.Pascal:
                    if (newUnit.InSet(Unit.Bar, Unit.InchesOfMercury))
                        return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(originalUnit));
            }
            return false;
        }
        public static UnitValue ConvertToSIStandard(this UnitValue unitValue)
        {
            if (unitValue.Unit.InSet(SIStandards)) return unitValue.Clone();

            switch(unitValue.Unit)
            {
                case Unit.Feet:
                    return new UnitValue(Unit.Meter, unitValue.Value  * 0.3048);
                case Unit.NauticalMile:
                    return new UnitValue(Unit.Meter, unitValue.Value * 1852);
                case Unit.StatuteMile:
                    return new UnitValue(Unit.Meter, unitValue.Value * 1609.344);
                case Unit.FeetPerMinute:
                    return new UnitValue(Unit.MetersPerSecond, unitValue.Value * 0.00508);
                case Unit.Knots:
                    return new UnitValue(Unit.MetersPerSecond, unitValue.Value * 0.514444444);
                case Unit.Mach:
                    return new UnitValue(Unit.MetersPerSecond, unitValue.Value * 340.3);
                case Unit.KnotsPerSeond:
                    return new UnitValue(Unit.MetersPerSecondSquared, unitValue.Value * 0.514444444);
                case Unit.Celcius:
                    return new UnitValue(Unit.Kelvin, unitValue.Value + 273.15);
                case Unit.Fahrenheit:
                    return new UnitValue(Unit.Kelvin, (unitValue.Value + 459.67) * (5.0/9.0));
                case Unit.Bar:
                    return new UnitValue(Unit.Pascal, unitValue.Value * 100000);
                case Unit.InchesOfMercury:
                    return new UnitValue(Unit.Pascal, unitValue.Value * 3386.38816);
            }
            throw new NotSupportedException($"Conversion of {unitValue.Unit} to standard is not implemented");
        }
        public static Unit CorrespondingSIStandard(this Unit unit)
        {
            if (unit.InSet(SIStandards)) return unit;

            switch (unit)
            {
                case Unit.Feet:
                case Unit.NauticalMile:
                case Unit.StatuteMile:
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
            }
            throw new NotSupportedException($"Conversion of {unit} to standard is not implemented");
        }

        public static string StringRepresentation(this Unit unit)
        {
            switch (unit)
            {
                case Unit.Meter:
                    return "m";
                case Unit.Feet:
                    return "ft";
                case Unit.NauticalMile:
                    return "NM";
                case Unit.StatuteMile:
                    return "mi";
                case Unit.MetersPerSecond:
                    return "m/s";
                case Unit.FeetPerMinute:
                    return "ft/min";
                case Unit.Knots:
                    return "kn";
                case Unit.Mach:
                    return "mach";
                case Unit.MetersPerSecondSquared:
                    return "m/s^2";
                case Unit.KnotsPerSeond:
                    return "kn/s";
                case Unit.Second:
                    return "s";
                case Unit.Kelvin:
                    return "°K";
                case Unit.Celcius:
                    return "°C";
                case Unit.Fahrenheit:
                    return "°F";
                case Unit.Pascal:
                    return "Pa";
                case Unit.Bar:
                    return "bar";
                case Unit.InchesOfMercury:
                    return "inHg";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
            }
        }

        public static double In(this UnitValue unitValue, Unit unit)
        {
            return unitValue.ConvertTo(unit).Value;
        }

        public static UnitValue To(this double value, Unit unit)
        {
            return new UnitValue(unit, value);
        }

        public static UnitValue To(this float value, Unit unit)
        {
            return new UnitValue(unit, value);
        }

        public static UnitValue To(this int value, Unit unit)
        {
            return new UnitValue(unit, value);
        }

        public static UnitValue RoundToNearest(this UnitValue value, UnitValue resolution)
        {
            return Math.Round(value.ConvertTo(resolution.Unit).Value / resolution.Value) * resolution.Value.To(resolution.Unit);
        }
        public static UnitValue RoundDownToNearest(this UnitValue value, UnitValue resolution)
        {
            return Math.Floor(value.ConvertTo(resolution.Unit).Value / resolution.Value) * resolution.Value.To(resolution.Unit);
        }
        public static UnitValue RoundUpToNearest(this UnitValue value, UnitValue resolution)
        {
            return Math.Ceiling(value.ConvertTo(resolution.Unit).Value / resolution.Value) * resolution.Value.To(resolution.Unit);
        }

        public static UnitValue Sum<T>(this IEnumerable<T> items, Func<T, UnitValue> valueSelector, Unit unit)
        {
            return items.Select(valueSelector).Sum(unit);
        }
        public static UnitValue Sum(this IEnumerable<UnitValue> items, Unit unit)
        {
            return items.Select(item => item.ConvertTo(unit).Value).Sum().To(unit);
        }
    }
}
