using System;
using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO.Pdb
{
    public class PdbModel : IDisposable
    {
        public PdbModel(params Peptide[] chains)
        {
            Chains.AddRange(chains);
        }

        public List<Peptide> Chains { get; } = new List<Peptide>();

        public void Dispose()
        {
            Chains.ForEach(chain => chain.Dispose());
            Chains.Clear();
        }
    }
}