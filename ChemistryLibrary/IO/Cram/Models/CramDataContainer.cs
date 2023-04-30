using System.Collections.Generic;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramDataContainer
    {
        public CramContainerHeader ContainerHeader { get; }
        public CramCompressionHeader CompressionHeader { get; }
        public List<CramSlice> Slices { get; }
        public List<CramBlock> Blocks => Slices.SelectMany(x => x.ExternalBlocks).ToList();

        public CramDataContainer(
            CramContainerHeader containerHeader, 
            CramCompressionHeader compressionHeader, 
            List<CramSlice> slices)
        {
            ContainerHeader = containerHeader;
            CompressionHeader = compressionHeader;
            Slices = slices;
        }
    }
}