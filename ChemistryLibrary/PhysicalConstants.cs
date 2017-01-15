using Commons;

namespace ChemistryLibrary
{
    public static class PhysicalConstants
    {
        public static UnitValue ProtonMass => 1.6726219 * 1e-27.To(Unit.Kilogram);
        public static UnitValue NeutronMass => 1.674929 * 1e-27.To(Unit.Kilogram);
        public static UnitValue ElectronMass => 9.10938356*1e-31.To(Unit.Kilogram);
        public static UnitValue ElementaryCharge => 1.60217662*1e-19.To(Unit.Coulombs);
        public static UnitValue SpeedOfLight => 299792458.To(Unit.MetersPerSecond);
        public static double AvogradrosNumber => 6.02214086*1e23;
    }
}
