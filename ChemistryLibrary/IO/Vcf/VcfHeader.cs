using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfHeader
    {
        public IList<string> Columns { get; }

        public VcfHeader(IList<string> columns)
        {
            Columns = columns;
        }
    }
}