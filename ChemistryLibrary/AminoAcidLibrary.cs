using System;

namespace ChemistryLibrary
{
    public static class AminoAcidLibrary
    {
        public static Molecule Alanine()
        {
            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon)
                .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Glycin()
        {
            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule IsoLeucine()
        {
            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon)
                .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

            var sideChain2Builder = new MoleculeBuilder();
            sideChain2Builder.Start
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start).AddToCurrentAtom(ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain2Builder.Start).AddToCurrentAtom(ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Leucine()
        {
            var sideChain1Builder = new MoleculeBuilder();
            var sideChain11Builder = new MoleculeBuilder();
            sideChain11Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);
            var sideChain12Builder = new MoleculeBuilder();
            sideChain12Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen)
                .AddSideChain(sideChain11Builder.Start)
                .AddSideChain(sideChain12Builder.Start);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Proline()
        {
            var sideChain1Builder = new MoleculeBuilder();
            var sideChain1End = sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).ConnectTo(sideChain1End)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Valine()
        {
            var sideChain1Builder = new MoleculeBuilder();
            var sideChain11Builder = new MoleculeBuilder();
            sideChain11Builder.Start
                .Add(ElementName.Carbon)
                .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);
            var sideChain12Builder = new MoleculeBuilder();
            sideChain12Builder.Start
                .Add(ElementName.Carbon)
                .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen)
                .AddSideChain(sideChain11Builder.Start)
                .AddSideChain(sideChain12Builder.Start);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Phenylalanine()
        {
            throw new NotImplementedException();

            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .AddToCurrentAtom(ElementName.Oxygen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Tryptophan()
        {
            throw new NotImplementedException();

            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .AddToCurrentAtom(ElementName.Oxygen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Tyrosine()
        {
            throw new NotImplementedException();

            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .AddToCurrentAtom(ElementName.Oxygen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule AsparticAcid()
        {
            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .AddToCurrentAtom(ElementName.Oxygen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule GlutamicAcid()
        {
            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .AddToCurrentAtom(ElementName.Oxygen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Arginine()
        {
            var sideChain1Builder = new MoleculeBuilder();
            var sideChain11Builder = new MoleculeBuilder();
            sideChain11Builder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);
            var sideChain12Builder = new MoleculeBuilder();
            sideChain11Builder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                .Add(ElementName.Carbon)
                .AddSideChain(sideChain11Builder.Start)
                .AddSideChain(sideChain12Builder.Start, BondMultiplicity.Double);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Histidine()
        {
            throw new NotImplementedException();
        }

        public static Molecule Lysine()
        {
            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Serine()
        {
            var sideChain1Builder = new MoleculeBuilder();
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Threonine()
        {
            var sideChain1Builder = new MoleculeBuilder();;
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddSideChain(ElementName.Oxygen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Cysteine()
        {
            var sideChain1Builder = new MoleculeBuilder(); ;
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Sulfur, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Methionine()
        {
            var sideChain1Builder = new MoleculeBuilder(); ;
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Sulfur)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Asparagine()
        {
            var sideChain1Builder = new MoleculeBuilder(); ;
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }

        public static Molecule Glutamine()
        {
            var sideChain1Builder = new MoleculeBuilder(); ;
            sideChain1Builder.Start
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

            var moduleBuilder = new MoleculeBuilder();
            moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                .Add(ElementName.Oxygen, ElementName.Hydrogen);

            return moduleBuilder.Molecule;
        }
    }

    public class MoleculeBuilder
    {
        private MoleculeReference start;
        public Molecule Molecule { get; } = new Molecule();

        public MoleculeReference Start => start ?? (start = new MoleculeReference(Molecule));
    }

    public static class MoleculeExtensions
    {
        public static MoleculeReference Add(this MoleculeReference moleculeReference, params ElementName[] elements)
        {
            var lastAtomId = moleculeReference.LastAtomId;
            foreach (var element in elements)
            {
                var atom = Atom.FromStableIsotope(element);
                lastAtomId = moleculeReference.Molecule.AddAtom(atom, lastAtomId);
                if (!moleculeReference.IsInitialized)
                    moleculeReference.LastAtomId = lastAtomId;
            }
            return new MoleculeReference(moleculeReference.Molecule, lastAtomId);
        }
        public static MoleculeReference Add(this MoleculeReference moleculeReference, MoleculeReference otherMoleculeReference)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            moleculeReference.Molecule.AddMolecule(otherMoleculeReference, moleculeReference.LastAtomId);
            return otherMoleculeReference;
        }

        public static MoleculeReference AddToCurrentAtom(this MoleculeReference moleculeReference, params ElementName[] elements)
        {
            if(!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            foreach (var element in elements)
            {
                var atom = Atom.FromStableIsotope(element);
                moleculeReference.Molecule.AddAtom(atom, moleculeReference.LastAtomId);
            }
            return moleculeReference;
        }
        public static MoleculeReference AddToCurrentAtom(this MoleculeReference moleculeReference, ElementName element, BondMultiplicity bondMultiplicity)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            var atom = Atom.FromStableIsotope(element);
            moleculeReference.Molecule.AddAtom(atom, moleculeReference.LastAtomId, bondMultiplicity);
            return moleculeReference;
        }

        public static MoleculeReference AddSideChain(this MoleculeReference moleculeReference, params ElementName[] sideChainElements)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            var sideChainBuilder = new MoleculeBuilder();
            var sideChainReference = sideChainBuilder.Start.Add(sideChainElements);
            moleculeReference.Molecule.AddMolecule(sideChainReference, moleculeReference.LastAtomId, BondMultiplicity.Single);
            return moleculeReference;
        }

        public static MoleculeReference AddSideChain(this MoleculeReference moleculeReference, MoleculeReference sideChainReference, 
            BondMultiplicity bondMultiplicity = BondMultiplicity.Single)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            moleculeReference.Molecule.AddMolecule(sideChainReference, moleculeReference.LastAtomId, BondMultiplicity.Single);
            return moleculeReference;
        }

        public static MoleculeReference ConnectTo(this MoleculeReference moleculeReference, MoleculeReference otherReference)
        {
            if (!moleculeReference.IsInitialized)
                throw new InvalidOperationException("Cannot add atoms. Molecule reference is not initialized");
            if (otherReference.Molecule != moleculeReference.Molecule)
            {
                moleculeReference.Molecule.AddMolecule(otherReference, moleculeReference.LastAtomId);
            }
            else
            {
                moleculeReference.Molecule.ConnectAtoms(moleculeReference.LastAtomId, otherReference.LastAtomId);
            }
            return moleculeReference;
        }
    }

    public class MoleculeReference
    {
        private uint lastAtomId = uint.MaxValue;

        public MoleculeReference(Molecule molecule)
        {
            Molecule = molecule;
        }

        public MoleculeReference(Molecule molecule, uint lastAtomId)
            : this(molecule)
        {
            LastAtomId = lastAtomId;
        }

        public Molecule Molecule { get; }
        public bool IsInitialized { get; private set; }
        public uint LastAtomId
        {
            get { return lastAtomId; }
            set
            {
                if(IsInitialized)
                    throw new InvalidOperationException(nameof(LastAtomId) + " can only be set once");
                lastAtomId = value;
                IsInitialized = true;
            }
        }
    }
}
