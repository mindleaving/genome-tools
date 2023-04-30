using System;
using System.Collections.Generic;
using System.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramDataContainerReader
    {
        public CramDataContainer Read(CramBinaryReader reader, long containerOffset)
        {
            if(reader.Position != containerOffset)
                reader.Seek(containerOffset, SeekOrigin.Begin);
            var containerHeaderReader = new CramContainerHeaderReader();
            var containerHeader = containerHeaderReader.Read(reader);
            var sliceOffset = reader.Position;
            var compressionHeaderReader = new CramCompressionHeaderReader();
            var compressionHeader = compressionHeaderReader.Read(reader);
            var sliceReader = new CramSliceReader();
            var slices = sliceReader.ReadMany(reader, sliceOffset, containerHeader, compressionHeader);
            return new CramDataContainer(containerHeader, compressionHeader, slices);
        }

        public List<CramDataContainer> ReadMany(CramBinaryReader reader, long firstContainerOffset, int? count = null)
        {
            if(reader.Position != firstContainerOffset)
                reader.Seek(firstContainerOffset, SeekOrigin.Begin);
            var dataContainers = new List<CramDataContainer>();
            while (!count.HasValue || dataContainers.Count < count)
            {
                var dataContainer = Read(reader, reader.Position);
                if (IsEofContainer(dataContainer))
                {
                    dataContainers.Add(new CramEofContainer());
                    return dataContainers;
                }
                dataContainers.Add(dataContainer);
            }
            throw new Exception($"CRAM didn't contain {count.Value} containers and didn't contain an End Of File-container");
        }

        private bool IsEofContainer(CramDataContainer dataContainer)
        {
            return dataContainer.ContainerHeader.NumberOfBlocks == 1
                && dataContainer.ContainerHeader.Checksum == 1339669765
                && dataContainer.CompressionHeader.Checksum == 1258382318;
        }
    }
}