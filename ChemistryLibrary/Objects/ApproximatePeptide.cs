using System.Collections.Generic;
using System.Linq;
using ChemistryLibrary.Builders;
using Commons.Physics;

namespace ChemistryLibrary.Objects
{
    public class ApproximatePeptide
    {
        public List<ApproximatedAminoAcid> AminoAcids { get; } = new List<ApproximatedAminoAcid>();

        public ApproximatePeptide()
        {
        }

        public ApproximatePeptide(IList<ApproximatedAminoAcid> aminoAcids)
        {
            AminoAcids.AddRange(aminoAcids);
        }

        public void UpdatePositions()
        {
            ApproximateAminoAcidPositioner.Position(AminoAcids, new UnitPoint3D(Unit.Meter, 0, 0, 0));
        }

        public ApproximatePeptide DeepClone()
        {
            return new ApproximatePeptide(AminoAcids.Select(aa => aa.DeepClone()).ToList());
        }

        public void Add(ApproximatedAminoAcid aminoAcid)
        {
            AminoAcids.Add(aminoAcid);
        }
    }
}
