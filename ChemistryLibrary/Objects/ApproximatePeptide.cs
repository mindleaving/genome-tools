using System.Collections.Generic;
using ChemistryLibrary.Builders;
using Commons;

namespace ChemistryLibrary.Objects
{
    public class ApproximatePeptide
    {
        public List<ApproximatedAminoAcid> AminoAcids { get; } = new List<ApproximatedAminoAcid>();

        public ApproximatePeptide(IList<ApproximatedAminoAcid> aminoAcids)
        {
            AminoAcids.AddRange(aminoAcids);
        }

        public void UpdatePositions()
        {
            ApproximateAminoAcidPositioner.Position(AminoAcids, new UnitPoint3D(Unit.Meter, 0, 0, 0));
        }
    }
}
