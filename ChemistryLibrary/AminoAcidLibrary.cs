using System.Linq;

namespace ChemistryLibrary
{
    public static class AminoAcidLibrary
    {
        public static MoleculeReference Alanine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon)
                    .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Glycine
        {
            get
            {
                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference IsoLeucine
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain2Builder.Start).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Leucine
        {
            get
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Proline
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var sideChain1End = sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen)
                    .AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).ConnectTo(sideChain1End)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Valine
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Phenylalanine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .AddBenzolRing();

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Tryptophan
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var sideChain11Builder = new MoleculeBuilder();
                var sideChain11End = sideChain11Builder.Start
                    .Add(ElementName.Carbon, BondMultiplicity.Double)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen);
                var benzolReferences = sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .AddBenzolRing(1);
                benzolReferences.Single().ConnectTo(sideChain11End);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Tyrosine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                var benzolReferences = sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .AddBenzolRing(3);
                benzolReferences.Single().Add(ElementName.Oxygen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference AsparticAcid
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                    .AddToCurrentAtom(ElementName.Oxygen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference GlutamicAcid
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Arginine
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Histidine
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Lysine
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Serine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder();
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Oxygen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Threonine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddSideChain(ElementName.Oxygen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Cysteine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Sulfur, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Methionine
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Asparagine
        {
            get
            {
                var sideChain1Builder = new MoleculeBuilder(); ;
                sideChain1Builder.Start
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double)
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen);

                var moduleBuilder = new MoleculeBuilder();
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }

        public static MoleculeReference Glutamine
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
                return moduleBuilder.Start
                    .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                    .Add(ElementName.Carbon).AddSideChain(sideChain1Builder.Start)
                    .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            }
        }
    }
}
