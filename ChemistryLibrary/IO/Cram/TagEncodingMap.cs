using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class TagEncodingMap
    {
        public Dictionary<TagId, ICramEncoding<byte[]>> TagValueEncodings { get; }

        public TagEncodingMap()
        {
            TagValueEncodings = new Dictionary<TagId, ICramEncoding<byte[]>>();
        }
        public TagEncodingMap(Dictionary<TagId, ICramEncoding<byte[]>> tagValueEncodings)
        {
            TagValueEncodings = tagValueEncodings;
        }
    }
}