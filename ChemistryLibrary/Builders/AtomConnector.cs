using System.Collections.Generic;
using System.Linq;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
{
    public static class AtomConnector
    {
        public static IEnumerable<SimpleBond> CreateBonds(Atom atom1, Atom atom2, BondMultiplicity bondMultiplicity)
        {
            if(atom1 is AtomWithOrbitals && atom2 is AtomWithOrbitals)
                return CreateOrbitalBonds((AtomWithOrbitals)atom1, (AtomWithOrbitals)atom2, (int) bondMultiplicity);
            return CreateSimpleBonds(atom1, atom2, (int) bondMultiplicity);
        }

        private static IEnumerable<OrbitalBond> CreateOrbitalBonds(AtomWithOrbitals atom1, AtomWithOrbitals atom2, int bondCount)
        {
            if(!atom1.OrbitalsAvailableForBonding.Any())
                throw new ChemistryException($"Cannot bond to noble gas {atom1.Element}");
            if (!atom2.OrbitalsAvailableForBonding.Any())
                throw new ChemistryException($"Cannot bond to noble gas {atom2.Element}");
            var bonds = new List<OrbitalBond>();
            for (int bondIdx = 1; bondIdx <= bondCount; bondIdx++)
            {
                var orbital1 = atom1.OrbitalsAvailableForBonding.MinimumItem(o => o.Energy.In(Unit.ElectronVolts));
                var orbital2 = atom2.OrbitalsAvailableForBonding.MinimumItem(o => o.Energy.In(Unit.ElectronVolts));
                var bond = new OrbitalBond(atom1, orbital1, atom2, orbital2);
                bonds.Add(bond);
            }
            return bonds;
        }

        private static IEnumerable<SimpleBond> CreateSimpleBonds(Atom atom1, Atom atom2, int bondCount)
        {
            var bonds = new List<SimpleBond>();
            for (var bondIdx = 1; bondIdx <= bondCount; bondIdx++)
            {
                var bond = new SimpleBond(atom1, atom2);
                bonds.Add(bond);
            }
            return bonds;
        }
    }
}