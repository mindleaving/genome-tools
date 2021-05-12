using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.Objects
{
    public class Intron
    {
        public Intron(
            int startNucelotideIndex,
            List<Nucleotide> nucelotides)
        {
            StartNucelotideIndex = startNucelotideIndex;
            Nucelotides = nucelotides;
        }

        public int StartNucelotideIndex { get; }
        public List<Nucleotide> Nucelotides { get; }
    }
}