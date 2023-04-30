using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfLoaderResult
    {
        public List<VcfMetadataEntry> MetadataEntries { get; }
        public List<VcfVariantEntry> Variants { get; }

        public VcfLoaderResult(List<VcfMetadataEntry> metadataEntries, List<VcfVariantEntry> variants)
        {
            MetadataEntries = metadataEntries;
            Variants = variants;
        }
    }
}