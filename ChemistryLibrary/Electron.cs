using Commons;

namespace ChemistryLibrary
{
    public class Electron
    {
        public UnitValue Mass => PhysicalConstants.ElectronMass;
        public UnitValue Charge => -PhysicalConstants.ElementaryCharge;
        public SpinType Spin { get; set; }
        public UnitValue Energy => AssociatedOrbital?.Energy ?? 0.To(Unit.ElectronVolts);

        public Orbital AssociatedOrbital { get; set; }
        public Bond AssociatedBond { get; set; }
    }
}