using System;
using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Objects;
using Commons;

namespace ChemistryLibrary.Simulation
{
    public class SimulationTimestepCompleteEventArgs : EventArgs
    {
        private SimulationTimestepCompleteEventArgs(UnitValue t)
        {
            SimulationTime = t;
        }

        public SimulationTimestepCompleteEventArgs(UnitValue t, ApproximatePeptide peptide)
            : this(t)
        {
            PeptideCopy = peptide.DeepClone();
            Atoms = ToAtoms(PeptideCopy);
        }

        private List<Atom> ToAtoms(ApproximatePeptide peptide)
        {
            var atoms = new List<Atom>();
            foreach (var aminoAcid in peptide.AminoAcids)
            {
                var nitrogenAtom = Atom.FromStableIsotope(ElementName.Nitrogen);
                nitrogenAtom.Position = aminoAcid.NitrogenPosition;
                var carbonAlphaAtom = Atom.FromStableIsotope(ElementName.Carbon);
                carbonAlphaAtom.Position = aminoAcid.CarbonAlphaPosition;
                var carbonAtom = Atom.FromStableIsotope(ElementName.Carbon);
                carbonAtom.Position = aminoAcid.CarbonPosition;
                atoms.Add(nitrogenAtom);
                atoms.Add(carbonAlphaAtom);
                atoms.Add(carbonAtom);
            }
            return atoms;
        }

        public SimulationTimestepCompleteEventArgs(UnitValue t, Molecule molecule)
            : this(t)
        {
            Atoms = molecule.Atoms.ToList();
        }

        public UnitValue SimulationTime { get; }
        public ApproximatePeptide PeptideCopy { get; }
        public List<Atom> Atoms { get; }
    }
}