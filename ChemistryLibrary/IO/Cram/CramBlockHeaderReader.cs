using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramBlockHeaderReader
    {
        public CramBlockHeader Read(CramReader reader)
        {
            var compressionMethod = (CramBlock.CompressionMethod)reader.ReadByte();
            var contentType = (CramBlock.BlockContentType)reader.ReadByte();
            var contentId = reader.ReadItf8();
            var compressedSize = reader.ReadItf8();
            var uncompressedSize = reader.ReadItf8();
            return new CramBlockHeader(
                compressionMethod,
                contentType,
                contentId,
                compressedSize,
                uncompressedSize);
        }
    }
}