using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
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