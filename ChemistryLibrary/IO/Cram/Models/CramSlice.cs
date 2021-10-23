using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramSlice
    {
        public List<CramBlock> Blocks { get; }

        public CramSlice(List<CramBlock> blocks)
        {
            Blocks = blocks;
        }
    }
}