using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Cram.Index;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Sam;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramLoader
    {
        public CramLoaderResult Load(string filePath)
        {
            using var reader = new CramReader(filePath);

            var cramHeader = ReadCramHeaderContainer(reader);
            var dataContainers = ReadDataContainers(reader);
            if (!IsEofContainer(dataContainers.Last()))
                throw new FormatException("No EOF-container found. File is truncated");

            return new CramLoaderResult(cramHeader, dataContainers);
        }

        public CramHeader ReadCramHeaderContainer(string filePath)
        {
            using var reader = new CramReader(filePath);
            return ReadCramHeaderContainer(reader);
        }

        public CramHeader ReadCramHeaderContainer(CramReader reader)
        {
            var samHeaderParser = new SamHeaderParser();

            // TODO: Move to other class
            var containerHeaderReader = new CramContainerHeaderReader();
            var containerHeader = containerHeaderReader.Read(reader);

            var blockReader = new CramBlockReader();
            var blocks = blockReader.ReadBlocks(reader, containerHeader.NumberOfBlocks, null);
            var samHeader = Encoding.ASCII.GetString(blocks[0].UncompressedDecodedData);
            samHeader = samHeader.Substring(4); // TODO: Why are there 4 bytes before the header entries? String length?
            var samHeaderLines = samHeader.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var samHeaderEntries = samHeaderLines
                .Where(line => line.StartsWith("@"))
                .Select(samHeaderParser.Parse)
                .ToList();
            return new CramHeader(samHeaderEntries);
        }

        private List<CramDataContainer> ReadDataContainers(CramReader reader)
        {
            var dataContainers = new List<CramDataContainer>();
            while (true)
            {
                var dataContainer = ReadDataContainer(reader);
                if (IsEofContainer(dataContainer))
                {
                    dataContainers.Add(new CramEofContainer());
                    return dataContainers;
                }
                dataContainers.Add(dataContainer);
            }
        }

        private bool IsEofContainer(CramDataContainer dataContainer)
        {
            return dataContainer.ContainerHeader.NumberOfBlocks == 1
                && dataContainer.ContainerHeader.Checksum == 1339669765
                && dataContainer.CompressionHeader.Checksum == 1258382318;
        }

        private CramDataContainer ReadDataContainer(CramReader reader)
        {
            var containerHeaderReader = new CramContainerHeaderReader();
            var containerHeader = containerHeaderReader.Read(reader);
            var sliceOffset = reader.Position;
            var compressionHeaderReader = new CramCompressionHeaderReader();
            var compressionHeader = compressionHeaderReader.Read(reader);
            var slices = ReadSlices(reader, containerHeader, sliceOffset, compressionHeader);
            return new CramDataContainer(containerHeader, compressionHeader, slices);
        }

        private List<CramSlice> ReadSlices(CramReader reader, CramContainerHeader containerHeader, long sliceOffset, CramCompressionHeader compressionHeader)
        {
            var slices = new List<CramSlice>();
            var sliceReader = new CramSliceReader();
            foreach (var slicePosition in containerHeader.SlicePositions)
            {
                var slice = sliceReader.Read(reader, slicePosition, sliceOffset, compressionHeader);
                slices.Add(slice);
            }
            return slices;
        }

        

        public CramSlice LoadSlice(CramReader reader, CramIndexEntry indexEntry)
        {
            var containerHeaderReader = new CramContainerHeaderReader();
            var containerHeader = containerHeaderReader.Read(reader, indexEntry.AbsoluteContainerOffset);
            var sliceOffsetReference = reader.Position;

            var compressionHeaderReader = new CramCompressionHeaderReader();
            var compressionHeader = compressionHeaderReader.Read(reader);

            var sliceReader = new CramSliceReader();
            return sliceReader.Read(
                reader,
                indexEntry.RelativeSliceHeaderOffset,
                sliceOffsetReference,
                compressionHeader);
        }
    }
}