using System;
using System.Linq;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
{
    public static class AminoAcidLibrary
    {
        public static AminoAcidReference Get(AminoAcidName aminoAcidCode, int sequenceNumber)
        {
            switch (aminoAcidCode)
            {
                case AminoAcidName.Isoleucine:
                    return Isoleucine(sequenceNumber);
                case AminoAcidName.Leucine:
                    return Leucine(sequenceNumber);
                case AminoAcidName.Valine:
                    return Valine(sequenceNumber);
                case AminoAcidName.Phenylalanine:
                    return Phenylalanine(sequenceNumber);
                case AminoAcidName.Methionine:
                    return Methionine(sequenceNumber);
                case AminoAcidName.Cysteine:
                    return Cysteine(sequenceNumber);
                case AminoAcidName.Alanine:
                    return Alanine(sequenceNumber);
                case AminoAcidName.Glycine:
                    return Glycine(sequenceNumber);
                case AminoAcidName.Proline:
                    return Proline(sequenceNumber);
                case AminoAcidName.Threonine:
                    return Threonine(sequenceNumber);
                case AminoAcidName.Serine:
                    return Serine(sequenceNumber);
                case AminoAcidName.Tyrosine:
                    return Tyrosine(sequenceNumber);
                case AminoAcidName.Tryptophan:
                    return Tryptophan(sequenceNumber);
                case AminoAcidName.Glutamine:
                    return Glutamine(sequenceNumber);
                case AminoAcidName.Asparagine:
                    return Asparagine(sequenceNumber);
                case AminoAcidName.Histidine:
                    return Histidine(sequenceNumber);
                case AminoAcidName.GlutamicAcid:
                    return GlutamicAcid(sequenceNumber);
                case AminoAcidName.AsparticAcid:
                    return AsparticAcid(sequenceNumber);
                case AminoAcidName.Lysine:
                    return Lysine(sequenceNumber);
                case AminoAcidName.Arginine:
                    return Arginine(sequenceNumber);
                default:
                    throw new ArgumentOutOfRangeException(nameof(aminoAcidCode), $"Unknown amino acid code '{aminoAcidCode}'");
            }
        }

        public static AminoAcidReference Alanine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Alanine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Glycine(int sequenceNumber)
        {
            var moduleBuilder = new MoleculeBuilder();
            var moleculeReference = moduleBuilder.Start
                .Add(ElementName.Nitrogen).AddToCurrentAtom(ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen)
                .Add(ElementName.Carbon).AddToCurrentAtom(ElementName.Oxygen, BondMultiplicity.Double);
            return new AminoAcidReference(AminoAcidName.Glycine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Isoleucine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Isoleucine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Leucine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Leucine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Proline(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Proline, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Valine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Valine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Phenylalanine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Phenylalanine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Tryptophan(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Tryptophan, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Tyrosine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Tyrosine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference AsparticAcid(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.AsparticAcid, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference GlutamicAcid(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.GlutamicAcid, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Arginine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Arginine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Histidine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Histidine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Lysine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Lysine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Serine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Serine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Threonine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Threonine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Cysteine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Cysteine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Methionine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Methionine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Asparagine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Asparagine, sequenceNumber, moleculeReference);
        }

        public static AminoAcidReference Glutamine(int sequenceNumber)
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
            return new AminoAcidReference(AminoAcidName.Glutamine, sequenceNumber, moleculeReference);
        }
    }
}
