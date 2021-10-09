using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfVariantEntry
    {
        public string Chromosome { get; }
        public string Position { get; }
        public string Id { get; }
        public string ReferenceBases { get; }
        public string AlternateBases { get; }
        public string Quality { get; }
        public VcfFilterResult FilterResult { get; }
        public Dictionary<string,string> Info { get; }
        public Dictionary<string, string> OtherFields { get; }

        public VcfVariantEntry(
            string chromosome, string position, string id,
            string referenceBases, string alternateBases, string quality,
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