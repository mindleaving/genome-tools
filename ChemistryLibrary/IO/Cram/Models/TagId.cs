namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class TagId
    {
        public string Tag { get; }
        public char ValueType { get; }

        public TagId(string tag, char valueType)
        {
            Tag = tag;
            ValueType = valueType;
        }
    }
}