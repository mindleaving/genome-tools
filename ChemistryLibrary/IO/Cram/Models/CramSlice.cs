using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramSlice
    {
        public CramContainerHeader ContainerHeader { get; }
        public CramCompressionHeader CompressionHeader { get; }
        public CramSliceHeader Header { get; }
        public CramBlock CoreDataBlock { get; }
        public List<CramBlock> ExternalBlocks { get; }

        public CramSlice(
            CramContainerHeader containerHeader, 
            CramCompressionHeader compressionHeader,
            CramSliceHeader header, 
            CramBlock coreDataBlock, 
            List<CramBlock> externalBlocks)
        {
            ExternalBlocks = externalBlocks;
            ContainerHeader = containerHeader;
            CompressionHeader = compressionHeader;
            Header = header;
            CoreDataBlock = coreDataBlock;
        }
    }
}