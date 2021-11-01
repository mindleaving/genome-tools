using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramBlockReader
    {
        public CramBlock Read(CramBinaryReader reader, CramCompressionHeader compressionHeader)
        {
            var blockHeaderReader = new CramBlockHeaderReader();
            var blockHeader = blockHeaderReader.Read(reader);
            var compressedData = reader.ReadBytes(blockHeader.CompressedSize);
            var blockCompressor = new CramBlockCompressor();
            var uncompressedData = blockCompressor.Decompress(compressedData, blockHeader);
            var checksum = reader.ReadInt32();

            return new CramBlock(blockHeader, uncompressedData, checksum);
        }

        public List<CramBlock> ReadBlocks(
            CramBinaryReader reader, 
            int numberOfBlocks,
            CramCompressionHeader compressionHeader)
        {
            var blocks = new List<CramBlock>();
            for (int i = 0; i < numberOfBlocks; i++)
            {
                var block = Read(reader, compressionHeader);
                blocks.Add(block);
            }
            return blocks;
        }
    }
}
