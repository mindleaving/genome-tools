using ChemistryLibrary.DataLookups;
using Commons.Extensions;
using Commons.Physics;

namespace ChemistryLibrary.Objects
{
    public class Electron
    {
        public UnitValue Mass => PhysicalConstants.ElectronMass;
        public UnitValue Charge => -PhysicalConstants.ElementaryCharge;
        public SpinType Spin { get; set; }
        public UnitValue Energy => AssociatedOrbital?.Energy ?? 0.To(Unit.ElectronVolts);

        public Orbital AssociatedOrbital { get; set; }
        public OrbitalBond AssociatedBond { get; set; }
    }
}