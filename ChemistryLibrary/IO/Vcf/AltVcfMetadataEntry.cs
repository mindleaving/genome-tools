namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class AltVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.Alt;
        public string Id { get; }
        public string Description { get; }

        public AltVcfMetadataEntry(string id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}