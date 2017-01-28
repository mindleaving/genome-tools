﻿using System;
using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public class Orbital
    {
        public Orbital(Atom atom, int period, OrbitalType type)
        {
            Atom = atom;
            Period = period;
            Type = type;
        }

        public Atom Atom { get; }
        public OrbitalType Type { get; }
        public int Period { get; }
        public List<Electron> Electrons { get; } = new List<Electron>(2);
        public UnitValue Energy => AtomPropertyCalculator.CalculateOrbitalEnergy(Atom, this);
        public bool IsFull => Electrons.Count == 2;
        public bool IsEmpty => Electrons.Count == 0;
        public bool IsOuterOrbital => Period >= Atom.Period;
        public Bond AssociatedBond { get; set; }

        private Point3D electronDensityMaximumPosition;
        public Point3D MaximumElectronDensityPosition
        {
            get
            {
                if (AssociatedBond != null)
                {
                    var atom1 = AssociatedBond.Atom1;
                    var atom2 = AssociatedBond.Atom2;
                    var chargeImbalance = atom1.ElectroNegativity/(atom1.ElectroNegativity + atom2.ElectroNegativity);
                    return new UnitPoint3D(atom1.Position.Unit,
                        chargeImbalance * atom1.Position.X + (1 - chargeImbalance) * atom2.Position.X,
                        chargeImbalance * atom1.Position.Y + (1 - chargeImbalance) * atom2.Position.Y,
                        chargeImbalance * atom1.Position.Z + (1 - chargeImbalance) * atom2.Position.Z);
                }
                if (!IsOuterOrbital)
                    return Atom.Position;
                return electronDensityMaximumPosition;
            }
            set
            {
                if(AssociatedBond != null)
                    throw new InvalidOperationException("Cannot set electron density maximum position for orbit part of a bond");
                if(!IsOuterOrbital)
                    throw new InvalidOperationException("Cannot set electron density maximum position for non-outer orbital");
                electronDensityMaximumPosition = value;
            }
        }
        //public Vector3D Force { get; set; } // Force applied to electron distribution. Used to position lone pairs correctly relative to other bonds

        public void AddElectron(Electron electron)
        {
            if (IsFull)
                throw new ChemistryException("Adding a third electron to an orbital is forbidden");
            if(electron.AssociatedOrbital != null)
                throw new ChemistryException("Cannot add electron to orbital which is already associated to another orbital");
            if (!IsEmpty)
            {
                var existingElectron = Electrons[0];
                if (existingElectron.Spin == electron.Spin)
                    electron.Spin = existingElectron.Spin.Invert();
            }
            Electrons.Add(electron);
            electron.AssociatedOrbital = this;
        }

        public void AddBondElectron(Electron electron)
        {
            if(IsFull)
                throw new ChemistryException("Adding a third electron to an orbital is forbidden");
            if(IsEmpty)
                throw new ChemistryException("Adding a bond electron to an empty orbital sounds like an error...");
            var existingElectron = Electrons[0];
            if (existingElectron.Spin == electron.Spin)
                electron.Spin = existingElectron.Spin.Invert();
            Electrons.Add(electron);
        }

        public Electron RemoveElectron()
        {
            if(!Electrons.Any())
                throw new ChemistryException("Cannot remove electron from empty orbital");
            var downSpinElectron = Electrons.SingleOrDefault(e => e.Spin == SpinType.Down);
            if (downSpinElectron != null)
            {
                Electrons.Remove(downSpinElectron);
                return downSpinElectron;
            }
            var upSpinElectron = Electrons[0];
            Electrons.Remove(upSpinElectron);
            return upSpinElectron;
        }

        public void BreakBond()
        {
            Electrons.RemoveAll(electron => electron.AssociatedOrbital != this);
            Electrons.ForEach(e => e.AssociatedBond = null);
            AssociatedBond = null;
        }

        public override string ToString()
        {
            return $"{Period}{Type}^{Electrons.Count}";
        }
    }

    public enum OrbitalType
    {
        s = 1,
        p = 2,
        d = 3,
        f = 4
    }
}