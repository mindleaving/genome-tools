using System;
using System.Collections.Generic;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Simulation
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
            Atoms = AtomExtractor.FromApproximatePeptide(peptide);
            Atoms.ForEach(atom => atom.IsBackbone = true);
        }

        public SimulationTimestepCompleteEventArgs(UnitValue t, Molecule molecule)
            : this(t)
        {
            Atoms = AtomExtractor.FromMolecule(molecule);
        }

        public UnitValue SimulationTime { get; }
        public ApproximatePeptide PeptideCopy { get; }
        public List<Atom> Atoms { get; }
    }
}