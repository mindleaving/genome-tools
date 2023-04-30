namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramBlock
    {
        public enum CompressionMethod
        {
            Raw = 0,
            Gzip = 1,
            Bzip2 = 2,
            Lzma = 3,
            Rans = 4
        }

        public enum BlockContentType
        {
            FileHeader = 0,
            CompressionHeader = 1,
            SliceHeader = 2,
            ExternalData = 4,
            CoreData = 5
        }

        public CramBlockHeader Header { get; }
        public int Checksum { get; }
        public byte[] UncompressedDecodedData { get; }

        public CramBlock(CramBlockHeader header, byte[] uncompressedDecodedData, int checksum)
        {
            Header = header;
            UncompressedDecodedData = uncompressedDecodedData;
            Checksum = checksum;
        }
    }
}