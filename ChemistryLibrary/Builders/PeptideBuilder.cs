using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.Builders
{
    public static class PeptideBuilder
    {
        public static Peptide PeptideFromString(string peptideString, int firstSequenceNumber)
        {
            var sequence = Regex.Replace(peptideString.ToUpperInvariant(), "[^A-Z]", "")
                .Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName());
            return PeptideFromSequence(sequence, firstSequenceNumber);
        }

        public static Peptide PeptideFromSequence(IEnumerable<AminoAcidName> aminoAcidNameSequence, int firstSequenceNumber)
        {
            var aminoAcidReferences = aminoAcidNameSequence
                .Select((aminoAcidName, idx) => AminoAcidLibrary.Get(aminoAcidName, firstSequenceNumber + idx));
            return PeptideFromAminoAcids(aminoAcidReferences);
        }

        public static Peptide PeptideFromSequence(AminoAcidSequence aminoAcidNameSequence)
        {
            var aminoAcidReferences = aminoAcidNameSequence
                .Select(sequenceItem => AminoAcidLibrary.Get(sequenceItem.AminoAcidName, sequenceItem.ResidueNumber));
            return PeptideFromAminoAcids(aminoAcidReferences);
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
}
