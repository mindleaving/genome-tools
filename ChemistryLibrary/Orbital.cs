using System.Collections.Generic;
using System.Linq;
using Commons;

namespace ChemistryLibrary
{
    public class Orbital
    {
        public Atom Atom { get; }

        public Orbital(Atom atom, int period, OrbitalType type)
        {
            Atom = atom;
            Period = period;
            Type = type;
        }

        public OrbitalType Type { get; }
        public int Period { get; }
        public List<Electron> Electrons { get; } = new List<Electron>(2);
        public UnitValue Energy => AtomPropertyCalculator.CalculateOrbitalEnergy(Atom, this);
        public bool IsFull => Electrons.Count == 2;

        public void AddElectron(Electron electron)
        {
            if (Electrons.Count == 2)
                throw new ChemistryException("Adding a third electron to an orbital is forbidden");
            if (Electrons.Any())
            {
                var existingElectron = Electrons[0];
                if (existingElectron.Spin == electron.Spin)
                    electron.Spin = existingElectron.Spin.Invert();
            }
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
    }

    public enum OrbitalType
    {
        s,
        p,
        d,
        f
    }
}