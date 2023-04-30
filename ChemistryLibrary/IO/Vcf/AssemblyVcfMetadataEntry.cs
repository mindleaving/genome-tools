namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class AssemblyVcfMetadataEntry : VcfMetadataEntry
    {
        public AssemblyVcfMetadataEntry(string url)
        {
            Url = url;
        }

        public override MetadataType EntryType => MetadataType.Assembly;
        public string Url { get; }
    }
}