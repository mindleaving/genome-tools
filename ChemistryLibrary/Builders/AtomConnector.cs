using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Physics;

namespace ChemistryLibrary.Builders
{
    public static class AtomConnector
    {
        public static IEnumerable<Bond> CreateBonds(Atom atom1, Atom atom2, BondMultiplicity bondMultiplicity)
        {
            return CreateBonds(atom1, atom2, (int) bondMultiplicity);
        }

        private static IEnumerable<Bond> CreateBonds(Atom atom1, Atom atom2, int bondCount)
        {
            if(!atom1.OrbitalsAvailableForBonding.Any())
                throw new ChemistryException($"Cannot bond to noble gas {atom1.Element}");
            if (!atom2.OrbitalsAvailableForBonding.Any())
                throw new ChemistryException($"Cannot bond to noble gas {atom2.Element}");
            var bonds = new List<Bond>();
            for (int bondIdx = 1; bondIdx <= bondCount; bondIdx++)
            {
                var orbital1 = atom1.OrbitalsAvailableForBonding.MinimumItem(o => o.Energy.In(Unit.ElectronVolts));
                var orbital2 = atom2.OrbitalsAvailableForBonding.MinimumItem(o => o.Energy.In(Unit.ElectronVolts));
                var bond = new Bond(atom1, orbital1, atom2, orbital2);
                bonds.Add(bond);
            }
            return bonds;
        }
    }
}