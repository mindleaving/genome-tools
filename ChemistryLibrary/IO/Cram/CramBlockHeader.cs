namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramBlockHeader
    {
        public CramBlock.CompressionMethod CompressionMethod { get; }
        public CramBlock.BlockContentType ContentType { get; }
        public int ContentId { get; }
        public int CompressedSize { get; }
        public int UncompressedSize { get; }

        public CramBlockHeader(
            CramBlock.CompressionMethod compressionMethod, 
            CramBlock.BlockContentType contentType, 
            int contentId,
            int compressedSize, 
            int uncompressedSize)
        {
            CompressionMethod = compressionMethod;
            ContentType = contentType;
            ContentId = contentId;
            CompressedSize = compressedSize;
            UncompressedSize = uncompressedSize;
        }
    }
}