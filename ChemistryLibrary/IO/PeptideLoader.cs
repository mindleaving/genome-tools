using System;
using System.IO;
using System.Linq;
using ChemistryLibrary.IO.Aminoseq;
using ChemistryLibrary.IO.Pdb;
using ChemistryLibrary.Objects;

namespace ChemistryLibrary.IO
{
    public static class PeptideLoader
    {
        public static Peptide Load(string filename)
        {
            var extension = Path.GetExtension(filename).ToLowerInvariant();
            switch (extension)
            {
                case "pdb":
                    var result = PdbReader.ReadFile(filename);
                    return result.Chains.First();
                case "aminoseq":
                    return AminoseqReader.ReadFile(filename);
                default:
                    throw new ArgumentException($"File extension '{extension}' is unsupported");
            }
        }
    }
}
