using System.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramContainerHeaderReader
    {
        public CramContainerHeader Read(CramBinaryReader reader, long? offset = null)
        {
            if(offset.HasValue && reader.Position != offset.Value)
                reader.Seek(offset.Value, SeekOrigin.Begin);

            var containerLength = reader.ReadInt32();
            var referenceSequenceId = reader.ReadItf8();
            var referenceStartingPosition = reader.ReadItf8();
            var alignmentSpan = reader.ReadItf8();
            var numberOfRecords = reader.ReadItf8();
            var recordCounter = reader.ReadLtf8();
            var numberOfReadBases = reader.ReadLtf8();
            var numberOfBlocks = reader.ReadItf8();
            var slicePositions = reader.ReadCramItf8Array();
            var checksum = reader.ReadInt32();
            return new CramContainerHeader(
                containerLength,
                referenceSequenceId,
                referenceStartingPosition,
                alignmentSpan,
                numberOfRecords,
                recordCounter,
                numberOfReadBases,
                numberOfBlocks,
                checksum,
                slicePositions);
        }
    }
}
