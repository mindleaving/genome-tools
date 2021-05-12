﻿using System.Linq;
using Commons.Extensions;
using Commons.Physics;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class OrbitalBond : SimpleBond
    {
        public OrbitalBond(
            AtomWithOrbitals atom1, 
            Orbital orbital1, 
            AtomWithOrbitals atom2, 
            Orbital orbital2)
            : base(atom1, atom2)
        {
            if (orbital1.AssociatedBond != null
                || orbital2.AssociatedBond != null)
            {
                throw new ChemistryException("Cannot create bond between orbits which are already assicated to a bond");
            }
            if (orbital1.IsEmpty || orbital2.IsEmpty)
            {
                throw new ChemistryException("Cannot create bond between orbitals if any is empty");
            }
            if (orbital1.IsFull || orbital2.IsFull)
            {
                throw new ChemistryException("Cannot create bond between orbitals if any is full");
            }
            Orbital1 = orbital1;
            Orbital2 = orbital2;

            ShareElectrons(Orbital1, Orbital2);
        }

        public Orbital Orbital1 { get; }
        public Orbital Orbital2 { get; }

        public UnitValue BondEnergy { get; set; }
        /// <summary>
        /// Bond length, measured as distance from core to core
        /// </summary>
        public UnitValue BondLength { get; set; }

        private void ShareElectrons(Orbital orbital1, Orbital orbital2)
        {
            var electron1 = orbital1.Electrons.Single();
            var electron2 = orbital2.Electrons.Single();

            orbital1.AddBondElectron(electron2);
            orbital2.AddBondElectron(electron1);

            orbital1.AssociatedBond = this;
            orbital2.AssociatedBond = this;

            electron1.AssociatedBond = this;
            electron2.AssociatedBond = this;

            var electroNegativityDifference = orbital2.Atom.ElectroNegativity - orbital1.Atom.ElectroNegativity;
            Atom1.EffectiveCharge += (electroNegativityDifference/4.0).To(Unit.ElementaryCharge);
            Atom2.EffectiveCharge -= (electroNegativityDifference/4.0).To(Unit.ElementaryCharge);
        }

        private bool isBroken;
        public void BreakBond()
        {
            if(isBroken)
                throw new ChemistryException("Bond cannot be broken twice");
            Orbital1.BreakBond();
            Orbital2.BreakBond();
            var electroNegativityDifference = Orbital2.Atom.ElectroNegativity - Orbital1.Atom.ElectroNegativity;
            Atom1.EffectiveCharge -= (electroNegativityDifference / 4.0).To(Unit.ElementaryCharge);
            Atom2.EffectiveCharge += (electroNegativityDifference / 4.0).To(Unit.ElementaryCharge);

            isBroken = true;
        }
        
    }
}
