namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public abstract class VcfMetadataEntry
    {
        public enum MetadataType
        {
            FileFormat,
            Info,
            Format,
            Filter,
            Alt,
            Assembly,
            Contig,
            Sample,
            Pedigree,
            Custom
        }
        public abstract MetadataType EntryType { get; }
    }
}