using System.Linq;

namespace ChemistryLibrary
{
    public static class AminoAcidLibrary
    {
        public static AminoAcidReference Alanine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon)
                    .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Alanine, moleculeReference);
            }
        }

        public static AminoAcidReference Glycine
        {
            get
            {
                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Glycine, moleculeReference);
            }
        }

        public static AminoAcidReference Isoleucine
        {
            get
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
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain2Builder.Start).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Isoleucine, moleculeReference);
            }
        }

        public static AminoAcidReference Leucine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var sideChain11Builder = new MoleculeBuilder();
                sideChain11Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen)
                    .AddSideChain(sideChain11Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen); ;

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Leucine, moleculeReference);
            }
        }

        public static AminoAcidReference Proline
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var sideChain1End = sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen)
                    .AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).ConnectTo(sideChain1End)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Proline, moleculeReference);
            }
        }

        public static AminoAcidReference Valine
        {
            get
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
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Valine, moleculeReference);
            }
        }

        public static AminoAcidReference Phenylalanine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .AddBenzolRing();

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Phenylalanine, moleculeReference);
            }
        }

        public static AminoAcidReference Tryptophan
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var sideChain11Builder = new MoleculeBuilder();
                var sideChain11End = sideChain11Builder.Start
                    .Add(ElementName.Carbon)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen);
                var benzolReferences = sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain11Builder.Start)
                    .AddBenzolRing(1);
                benzolReferences.Single().ConnectTo(sideChain11End);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Tryptophan, moleculeReference);
            }
        }

        public static AminoAcidReference Tyrosine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var benzolReferences = sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .AddBenzolRing(3);
                benzolReferences.Single().Add(ElementName.Oxygen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Tyrosine, moleculeReference);
            }
        }

        public static AminoAcidReference AsparticAcid
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                    .AddToCurrentAtom(ElementName.Oxygen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.AsparticAcid, moleculeReference);
            }
        }

        public static AminoAcidReference GlutamicAcid
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                    .AddToCurrentAtom(ElementName.Oxygen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.GlutamicAcid, moleculeReference);
            }
        }

        public static AminoAcidReference Arginine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var sideChain11Builder = new MoleculeBuilder();
                sideChain11Builder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);
                var sideChain12Builder = new MoleculeBuilder();
                sideChain12Builder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen);
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon)
                    .AddSideChain(sideChain11Builder.Start)
                    .AddSideChain(sideChain12Builder.Start, BondMultiplicity.Double);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Arginine, moleculeReference);
            }
        }

        public static AminoAcidReference Histidine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var ringBuilder = new MoleculeBuilder();
                var ringEnd = ringBuilder.Start
                    .Add(ElementName.Carbon)
                    .Add(ElementName.Nitrogen)
                    .Add(ElementName.Carbon, BondMultiplicity.Double)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen);
                ringBuilder.Start.ConnectTo(ringEnd, BondMultiplicity.Double);
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen)
                    .AddSideChain(ringBuilder.Start);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Histidine, moleculeReference);
            }
        }

        public static AminoAcidReference Lysine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Lysine, moleculeReference);
            }
        }

        public static AminoAcidReference Serine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Oxygen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Serine, moleculeReference);
            }
        }

        public static AminoAcidReference Threonine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddSideChain(ElementName.Oxygen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Threonine, moleculeReference);
            }
        }

        public static AminoAcidReference Cysteine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Sulfur, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Cysteine, moleculeReference);
            }
        }

        public static AminoAcidReference Methionine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Sulfur)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Methionine, moleculeReference);
            }
        }

        public static AminoAcidReference Asparagine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Asparagine, moleculeReference);
            }
        }

        public static AminoAcidReference Glutamine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                var moleculeReference = moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
                return new AminoAcidReference(AminoAcidName.Glutamine, moleculeReference);
            }
        }
    }
}
