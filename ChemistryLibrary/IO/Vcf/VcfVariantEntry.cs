using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfVariantEntry
    {
        public string Chromosome { get; }
        public int Position { get; }
        public string Id { get; }
        public string ReferenceBases { get; }
        public IList<string> AlternateBases { get; }
        public string Quality { get; }
        public VcfFilterResult FilterResult { get; }
        public Dictionary<string,string> Info { get; }
        public Dictionary<string, string> OtherFields { get; }
        public bool IsInsertion => AlternateBases.Any(x => x.Length > ReferenceBases.Length);
        public bool IsDeletion => AlternateBases.Any(x => x.Length < ReferenceBases.Length);
        public bool IsHeterogenous => AlternateBases.Count > 1;
        public bool IsSNP => ReferenceBases.Length == 1 && AlternateBases.Any(x => x.Length == 1);
        public int LongestInsertionLength => AlternateBases.Where(x => x.Length > ReferenceBases.Length).Max(x => x.Length - ReferenceBases.Length);
        public int LongestDeletionLength => AlternateBases.Where(x => x.Length < ReferenceBases.Length).Max(x => ReferenceBases.Length - x.Length);

        public VcfVariantEntry(
            string chromosome, 
            int position, 
            string id,
            string referenceBases, 
            IList<string> alternateBases, 
            string quality,
            VcfFilterResult filterResult, 
            Dictionary<string, string> info, 
            Dictionary<string, string> otherFields)
        {
            Chromosome = chromosome;
            Position = position;
            Id = id;
            ReferenceBases = referenceBases;
            AlternateBases = alternateBases;
            Quality = quality;
            FilterResult = filterResult;
            Info = info;
            OtherFields = otherFields;
        }
    }
}