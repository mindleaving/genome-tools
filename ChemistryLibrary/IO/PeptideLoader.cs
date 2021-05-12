using System;
using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.IO.Aminoseq;
using GenomeTools.ChemistryLibrary.IO.Pdb;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO
{
    public static class PeptideLoader
    {
        public static Peptide Load(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            switch (extension)
            {
                case ".pdb":
                    var result = PdbReader.ReadFile(filename);
                    return result.Models.First().Chains.First();
                case ".aminoseq":
                    return AminoseqReader.ReadFile(filename);
                default:
                    throw new ArgumentException($"File extension '{extension}' is unsupported");
            }
        }
    }
}
