namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class ContigVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.Contig;
        public string Id { get; }
        public string Length { get; }
        public string Value { get; }

        public ContigVcfMetadataEntry(string id, string length, string value)
        {
            Id = id;
            Length = length;
            Value = value;
        }
    }
}