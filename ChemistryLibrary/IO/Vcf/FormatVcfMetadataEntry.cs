namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class FormatVcfMetadataEntry : VcfMetadataEntry
    {
        public string Id { get; }
        public int Number { get; }
        public string ValueType { get; }
        public string Description { get; }

        public FormatVcfMetadataEntry(
            string id, int number, string valueType,
            string description)
        {
            Id = id;
            Number = number;
            ValueType = valueType;
            Description = description;
        }

        public override MetadataType EntryType => MetadataType.Format;
    }
}