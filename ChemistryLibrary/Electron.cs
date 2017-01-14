using Commons;

namespace ChemistryLibrary
{
    public class Electron
    {
        public UnitValue Mass => PhysicalConstants.ElectronMass;
        public UnitValue Charge => -PhysicalConstants.ElementaryCharge;
        public SpinType Spin { get; set; }
    }
}