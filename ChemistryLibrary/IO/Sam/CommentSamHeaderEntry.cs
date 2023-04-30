namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class CommentSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.Comment;
        public string Comment { get; }

        public CommentSamHeaderEntry(string comment)
        {
            Comment = comment;
        }
    }
}