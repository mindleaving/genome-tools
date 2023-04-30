using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class GenericVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType { get; }
        public string MetadataTypeString { get; }
        public Dictionary<string, string> KeyValuePairs { get; }
        public string Value { get; }

        public GenericVcfMetadataEntry(MetadataType metadataType, string metadataTypeString, Dictionary<string, string> keyValuePairs)
        {
            EntryType = metadataType;
            MetadataTypeString = metadataTypeString;
            KeyValuePairs = keyValuePairs;
        }

        public GenericVcfMetadataEntry(MetadataType metadataType, string metadataTypeString, string value)
        {
            EntryType = metadataType;
            MetadataTypeString = metadataTypeString;
            Value = value;
        }
    }
}