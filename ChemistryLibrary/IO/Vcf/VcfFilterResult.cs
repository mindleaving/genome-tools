using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfFilterResult
    {
        public bool Pass { get; }
        /// <summary>
        /// Null if <see cref="Pass"/> is true
        /// </summary>
        public IList<string> FailingFilters { get; }

        private VcfFilterResult(bool pass, IList<string> failingFilters)
        {
            Pass = pass;
            FailingFilters = failingFilters;
        }

        public static VcfFilterResult Success()
        {
            return new VcfFilterResult(true, null);
        }

        public static VcfFilterResult Failed(IList<string> failingFilters)
        {
            return new VcfFilterResult(false, failingFilters);
        }
    }
}