using System.Collections.Generic;

namespace ChemistryLibrary.Objects
{
    public class SinglePointPeptide
    {
        public List<SinglePointAminoAcid> AminoAcids { get; } = new List<SinglePointAminoAcid>();
        public List<PeptideAnnotation<SinglePointAminoAcid>> Annotations { get; } = new List<PeptideAnnotation<SinglePointAminoAcid>>();

        public void Add(SinglePointAminoAcid aminoAcid)
        {
            AminoAcids.Add(aminoAcid);
        }
    }
}
