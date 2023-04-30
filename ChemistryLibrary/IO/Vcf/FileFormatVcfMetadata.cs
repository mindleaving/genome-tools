namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class FileFormatVcfMetadata : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.FileFormat;
        public string FileFormat { get; }

        public FileFormatVcfMetadata(string fileFormat)
        {
            FileFormat = fileFormat;
        }
    }
}