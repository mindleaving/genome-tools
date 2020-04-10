using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Builders
{
    public static class PeptideBuilder
    {
        public static Peptide PeptideFromString(string peptideString, PeptideBuilderOptions options = null)
        {
            var sequence = Regex.Replace(peptideString.ToUpperInvariant(), "[^A-Z]", "")
                .Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName());
            return PeptideFromSequence(sequence, options);
        }

        public static Peptide PeptideFromSequence(
            IEnumerable<AminoAcidName> aminoAcidNameSequence,
            PeptideBuilderOptions options = null)
        {
            if(options == null)
                options = new PeptideBuilderOptions();
            if (options.BuildMolecule)
            {
                var aminoAcidReferences = aminoAcidNameSequence
                    .Select((aminoAcidName, idx) => AminoAcidLibrary.Get(aminoAcidName, options.FirstSequenceNumber + idx));
                return PeptideFromAminoAcids(aminoAcidReferences);
            }
            else
            {
                var aminoAcidReferences = aminoAcidNameSequence
                    .Select((aa,idx) => new AminoAcidReference(aa, options.FirstSequenceNumber + idx, null, new List<uint>(), 0, 0))
                    .ToList();
                return new Peptide(new MoleculeReference(new Molecule()), aminoAcidReferences);
            }
        }

        public static Peptide PeptideFromSequence(AminoAcidSequence aminoAcidNameSequence, PeptideBuilderOptions options = null)
        {
            if(options == null)
                options = new PeptideBuilderOptions();
            if (options.BuildMolecule)
            {
                var aminoAcidReferences = aminoAcidNameSequence
                    .Select(sequenceItem => AminoAcidLibrary.Get(sequenceItem.AminoAcidName, sequenceItem.ResidueNumber));
                return PeptideFromAminoAcids(aminoAcidReferences);
            }
            else
            {
                var aminoAcidReferences = aminoAcidNameSequence
                    .Select((aa,idx) => new AminoAcidReference(aa.AminoAcidName, aa.ResidueNumber, null, new List<uint>(), 0, 0))
                    .ToList();
                return new Peptide(new MoleculeReference(new Molecule()), aminoAcidReferences);
            }
        }

        public static Peptide PeptideFromAminoAcids(IEnumerable<AminoAcidReference> aminoAcidReferences)
        {
            var aminoAcids = new List<AminoAcidReference>();
            MoleculeReference moleculeReference = null;
            foreach (var aminoAcid in aminoAcidReferences)
            {
                MoleculeReference aminoAcidReference;
                if (moleculeReference != null)
                {
                    moleculeReference = moleculeReference.Add(aminoAcid, out aminoAcidReference);
                }
                else
                {
                    moleculeReference = aminoAcid;
                    aminoAcidReference = new MoleculeReference(
                        aminoAcid.Molecule,
                        aminoAcid.VertexIds,
                        aminoAcid.FirstAtomId,
                        aminoAcid.LastAtomId);
                }
                aminoAcids.Add(new AminoAcidReference(aminoAcid.Name, aminoAcid.SequenceNumber, aminoAcidReference));
            }
            return new Peptide(moleculeReference, aminoAcids);
        }
    }

    public class PeptideBuilderOptions
    {
        public int FirstSequenceNumber { get; set; } = 1;
        public bool BuildMolecule { get; set; } = true;
    }
}
