using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Pdb;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
{
    /// <summary>
    /// Provides methods for building full molecules from an approximate peptide
    /// </summary>
    public class ApproximatePeptideCompleter
    {
        private readonly ApproximatePeptide approximatePeptide;

        public ApproximatePeptideCompleter(ApproximatePeptide approximatePeptide)
        {
            this.approximatePeptide = approximatePeptide;
        }

        public Peptide GetBackbone()
        {
            var aminoAcids = new List<AminoAcidReference>();
            foreach (var approximatedAminoAcid in approximatePeptide.AminoAcids)
            {
                var moleculeBuilder = new MoleculeBuilder();
                var aminoAcidReference = moleculeBuilder.Start
                    .Add(ElementName.Nitrogen, ElementName.Carbon, ElementName.Carbon);
                var aminoAcid = new AminoAcidReference(approximatedAminoAcid.Name, approximatedAminoAcid.SequenceNumber, aminoAcidReference);

                var nitrogen = aminoAcid.VertexIds
                    .Select(aminoAcid.Molecule.GetAtom)
                    .Single(atom => atom.Element == ElementName.Nitrogen);
                nitrogen.Position = approximatedAminoAcid.NitrogenPosition;
                nitrogen.IsPositionFixed = true;

                var carbonAlpha = aminoAcid.VertexIds
                    .Select(aminoAcid.Molecule.GetAtom)
                    .First(atom => atom.Element == ElementName.Carbon);
                carbonAlpha.Position = approximatedAminoAcid.CarbonAlphaPosition;
                carbonAlpha.IsPositionFixed = true;

                var carbon = aminoAcid.VertexIds
                    .Select(aminoAcid.Molecule.GetAtom)
                    .Last(atom => atom.Element == ElementName.Carbon);
                carbon.Position = approximatedAminoAcid.CarbonPosition;
                carbon.IsPositionFixed = true;

                aminoAcids.Add(aminoAcid);
            }

            var peptide = PeptideBuilder.PeptideFromAminoAcids(aminoAcids);
            return peptide;
        }

        public Peptide GetFullPeptide()
        {
            var aminoAcids = new List<AminoAcidReference>();
            foreach (var approximatedAminoAcid in approximatePeptide.AminoAcids)
            {
                var aminoAcid = AminoAcidLibrary.Get(approximatedAminoAcid.Name, approximatedAminoAcid.SequenceNumber);
                PdbAminoAcidAtomNamer.AssignNames(aminoAcid);

                var nitrogen = aminoAcid.GetAtomFromName("N");
                nitrogen.Position = approximatedAminoAcid.NitrogenPosition;
                nitrogen.IsPositionFixed = true;

                var carbonAlpha = aminoAcid.GetAtomFromName("CA");
                carbonAlpha.Position = approximatedAminoAcid.CarbonAlphaPosition;
                carbonAlpha.IsPositionFixed = true;

                var carbon = aminoAcid.GetAtomFromName("C");
                carbon.Position = approximatedAminoAcid.CarbonPosition;
                carbon.IsPositionFixed = true;

                aminoAcid.Molecule.PositionAtoms();
                aminoAcids.Add(aminoAcid);
            }

            var peptide = PeptideBuilder.PeptideFromAminoAcids(aminoAcids);
            peptide.Molecule.PositionAtoms();

            return peptide;
        }
    }
}
