using System.Collections.Generic;
using Commons.Extensions;
using Commons.Physics;

namespace GenomeTools.ChemistryLibrary.DataLookups
{
    public static class PhysicalConstants
    {
        public static UnitValue ProtonMass => 1.6726219 * 1e-27.To(Unit.Kilogram);
        public static UnitValue NeutronMass => 1.674929 * 1e-27.To(Unit.Kilogram);
        public static UnitValue ElectronMass => 9.10938356 * 1e-31.To(Unit.Kilogram);
        public static UnitValue ElementaryCharge => 1.60217662 * 1e-19.To(Unit.Coulombs);
        public static UnitValue CoulombsConstant => 8.987551 * 1e9.To(new CompoundUnit(
            new[] { SIBaseUnit.Kilogram, SIBaseUnit.Meter, SIBaseUnit.Meter, SIBaseUnit.Meter },
            new[] { SIBaseUnit.Second, SIBaseUnit.Second, SIBaseUnit.Ampere, SIBaseUnit.Second, SIBaseUnit.Ampere, SIBaseUnit.Second }));
        public static UnitValue SpeedOfLight => 299792458.To(Unit.MetersPerSecond);
        public static UnitValue AvogradrosNumber => 6.02214086 * 1e23.To(new CompoundUnit(new List<SIBaseUnit>(), new []{SIBaseUnit.Mole}));
        public static UnitValue BoltzmannsConstant => 1.38066 * 1e-23.To(CompoundUnits.Joule / CompoundUnits.Kelvin);
    }
}
