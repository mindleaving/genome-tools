namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public abstract class SamHeaderEntry
    {
        public enum HeaderEntryType
        {
            FileLevelMetadata,
            ReferenceSequence,
            ReadGroup,
            Program,
            Comment
        }

        public abstract HeaderEntryType Type { get; }
    }
}