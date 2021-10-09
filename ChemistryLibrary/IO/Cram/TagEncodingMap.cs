using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class TagEncodingMap
    {
        public Dictionary<TagId, CramEncoding> TagValueEncodings { get; }

        public TagEncodingMap()
        {
            TagValueEncodings = new Dictionary<TagId, CramEncoding>();
        }
        public TagEncodingMap(Dictionary<TagId, CramEncoding> tagValueEncodings)
        {
            TagValueEncodings = tagValueEncodings;
        }
    }
}