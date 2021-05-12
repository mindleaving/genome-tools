using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.Builders;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO.Aminoseq
{
    public static class AminoseqReader
    {
        public static Peptide ReadFile(string filename)
        {
            var aminoAcidSequence = File.ReadAllLines(filename).Aggregate((a, b) => a + b);
            return PeptideBuilder.PeptideFromString(aminoAcidSequence);
        }
    }
}
