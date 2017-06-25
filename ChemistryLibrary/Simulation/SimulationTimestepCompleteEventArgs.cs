﻿using System;
using System.Collections.Generic;
using ChemistryLibrary.Builders;
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
            Atoms = AtomExtractor.FromApproximatePeptide(peptide);
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