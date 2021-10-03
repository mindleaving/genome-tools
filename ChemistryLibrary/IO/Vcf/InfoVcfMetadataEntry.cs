using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class InfoVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.Info;
        public string Id { get; }
        public int Number { get; }
        public string ValueType { get; }
        public string Description { get; }
        public Dictionary<string, string> KeyValuePairs { get; }

        public InfoVcfMetadataEntry(
            string id, int number, string valueType,
            string description, Dictionary<string, string> keyValuePairs)
        {
            Id = id;
            Number = number;
            ValueType = valueType;
            Description = description;
            KeyValuePairs = keyValuePairs;
        }
    }
}