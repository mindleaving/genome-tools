using System;
using System.Linq;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramRawDataLoader
    {
        public CramRawData Load(string filePath, string referenceSequenceFilePath = null)
        {
            using var reader = new CramBinaryReader(filePath);
            reader.CheckFileFormat();
            var headerReader = new CramHeaderReader();

            var cramHeader = headerReader.Read(reader, reader.Position, referenceSequenceFilePath);
            var dataContainerReader = new CramDataContainerReader();
            var dataContainers = dataContainerReader.ReadMany(reader, reader.Position);
            if (dataContainers.Last() is not CramEofContainer)
                throw new FormatException("No EOF-container found. File is truncated");

            return new CramRawData(cramHeader, dataContainers);
        }
    }
}
