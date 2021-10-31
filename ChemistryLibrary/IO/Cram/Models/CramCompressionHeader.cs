using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramCompressionHeader
    {
        public PreservationMap PreservationMap { get; }
        public DataSeriesEncodingMap DataSeriesEncodingMap { get; }
        public Dictionary<TagId, ICramEncoding<byte[]>> TagEncodingMap { get; }
        public int Checksum { get; }

        public CramCompressionHeader(
            PreservationMap preservationMap, 
            DataSeriesEncodingMap dataSeriesEncodingMap, 
            Dictionary<TagId, ICramEncoding<byte[]>> tagEncodingMap, 
            int checksum)
        {
            PreservationMap = preservationMap;
            DataSeriesEncodingMap = dataSeriesEncodingMap;
            TagEncodingMap = tagEncodingMap;
            Checksum = checksum;
        }
    }
}