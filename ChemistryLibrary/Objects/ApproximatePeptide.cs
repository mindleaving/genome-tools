using System.Collections.Generic;
using System.Linq;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Builders;

namespace GenomeTools.ChemistryLibrary.Objects
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

        public void RemoveLast()
        {
            if(AminoAcids.Count == 0)
                return;
            AminoAcids.RemoveAt(AminoAcids.Count-1);
        }
    }
}
