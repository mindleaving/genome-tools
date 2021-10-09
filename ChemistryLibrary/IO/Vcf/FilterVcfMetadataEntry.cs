namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class FilterVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.Filter;
        public string Id { get; }
        public string Description { get; }

        public FilterVcfMetadataEntry(string id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}