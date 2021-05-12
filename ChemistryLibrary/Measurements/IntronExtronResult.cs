using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public class IntronExtronResult
    {
        public IntronExtronResult(List<Intron> introns,
            List<Exon> exons)
        {
            Introns = introns;
            Exons = exons;
        }

        public List<Intron> Introns { get; }
        public List<Exon> Exons { get; }
    }
}