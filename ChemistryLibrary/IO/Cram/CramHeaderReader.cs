using System;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using GenomeTools.ChemistryLibrary.IO.Sam;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramHeaderReader
    {
        public CramHeader Read(string filePath)
        {
            using var reader = new CramBinaryReader(filePath);
            reader.CheckFileFormat();
            return Read(reader, reader.Position);
        }

        public CramHeader Read(CramBinaryReader reader, long offset)
        {
            var samHeaderParser = new SamHeaderParser();

            // TODO: Move to other class
            var containerHeaderReader = new CramContainerHeaderReader();
            var containerHeader = containerHeaderReader.Read(reader, offset);

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
    }
}
