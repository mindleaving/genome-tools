namespace GenomeTools.ChemistryLibrary.IO.Cram
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