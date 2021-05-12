using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.IO.Pdb;
using GenomeTools.ChemistryLibrary.Measurements;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
{
    public static class ApproximatePeptideBuilder
    {
        public static ApproximatePeptide FromSequence(string sequence, int firstSequenceNumber)
        {
            var cleanedPeptideString = Regex.Replace(sequence.ToUpperInvariant(), "[^A-Z]", "");
            var aminoAcidNames = cleanedPeptideString.Select(c => c.ToAminoAcidName()).ToList();
            return FromSequence(aminoAcidNames, firstSequenceNumber);
        }

        public static ApproximatePeptide FromSequence(IList<AminoAcidName> sequence, int firstSequenceNumber)
        {
            var aminoAcids = sequence
                .Select((aaName, idx) => new ApproximatedAminoAcid(aaName, firstSequenceNumber+idx))
                .ToList();
            ApproximateAminoAcidPositioner.Position(aminoAcids, new UnitPoint3D(Unit.Meter, 0, 0, 0));
            return new ApproximatePeptide(aminoAcids);
        }

        public static ApproximatePeptide FromPeptide(Peptide peptide)
        {
            var dihedralAngles = AminoAcidAngleMeasurer.MeasureAngles(peptide);

            var approximateAminoAcids = new List<ApproximatedAminoAcid>();
            foreach (var aminoAcid in peptide.AminoAcids)
            {
                PdbAminoAcidAtomNamer.AssignNames(aminoAcid);
                var nitrogen = aminoAcid.GetAtomFromName("N");
                var carbonAlpha = aminoAcid.GetAtomFromName("CA");
                var carbon = aminoAcid.GetAtomFromName("C");
                var aminoAcidDihedralAngles = dihedralAngles[aminoAcid];

                var approximateAminoAcid = new ApproximatedAminoAcid(aminoAcid.Name, aminoAcid.SequenceNumber)
                {
                    NitrogenPosition = nitrogen.Position,
                    CarbonAlphaPosition = carbonAlpha.Position,
                    CarbonPosition = carbon.Position,
                    OmegaAngle = aminoAcidDihedralAngles.Omega,
                    PhiAngle = aminoAcidDihedralAngles.Phi,
                    PsiAngle = aminoAcidDihedralAngles.Psi,
                    IsFrozen = true
                };
                approximateAminoAcids.Add(approximateAminoAcid);
            }
            return new ApproximatePeptide(approximateAminoAcids);
        }
    }
}
