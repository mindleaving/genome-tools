using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfVariantEntry
    {
        public string PersonId { get; private set; }
        public string Chromosome { get; private set; }
        public int Position { get; private set; }
        public string Id { get; private set; }
        public string ReferenceBases { get; private set; }
        public IList<string> AlternateBases { get; private set; }
        public string Quality { get; private set; }
        public VcfFilterResult FilterResult { get; private set; }
        public Dictionary<string,string> Info { get; private set; }
        public Dictionary<string, string> OtherFields { get; private set; }
        public bool IsInsertion => AlternateBases.Any(x => x.Length > ReferenceBases.Length);
        public bool IsDeletion => AlternateBases.Any(x => x.Length < ReferenceBases.Length);
        public bool IsSNP => ReferenceBases.Length == 1 && AlternateBases.Any(x => x.Length == 1);
        public int LongestInsertionLength => AlternateBases.Where(x => x.Length > ReferenceBases.Length).Max(x => x.Length - ReferenceBases.Length);
        public int LongestDeletionLength => AlternateBases.Where(x => x.Length < ReferenceBases.Length).Max(x => ReferenceBases.Length - x.Length);

        public VcfVariantEntry() {}
        public VcfVariantEntry(
            string personId,
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
            PersonId = personId;
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