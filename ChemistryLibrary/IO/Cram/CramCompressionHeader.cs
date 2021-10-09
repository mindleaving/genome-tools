namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramCompressionHeader
    {
        public PreservationMap PreservationMap { get; }
        public DataSeriesEncodingMap DataSeriesEncodingMap { get; }
        public TagEncodingMap TagEncodingMap { get; }
        public int Checksum { get; }

        public CramCompressionHeader(
            PreservationMap preservationMap, 
            DataSeriesEncodingMap dataSeriesEncodingMap, 
            TagEncodingMap tagEncodingMap, 
            int checksum)
        {
            PreservationMap = preservationMap;
            DataSeriesEncodingMap = dataSeriesEncodingMap;
            TagEncodingMap = tagEncodingMap;
            Checksum = checksum;
        }
    }
}