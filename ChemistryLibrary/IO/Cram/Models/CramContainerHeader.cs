using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramContainerHeader
    {
        public int ContainerLength { get; }
        public int ReferenceSequenceId { get; }
        public int ReferenceStartingPosition { get; }
        public int AlignmentSpan { get; }
        public int NumberOfRecords { get; }
        public long RecordCounter { get; }
        public long NumberOfReadBases { get; }
        public int NumberOfBlocks { get; }
        public int Checksum { get; }
        public List<int> SlicePositions { get; }

        public CramContainerHeader(
            int containerLength, 
            int referenceSequenceId, 
            int referenceStartingPosition,
            int alignmentSpan, 
            int numberOfRecords, 
            long recordCounter,
            long numberOfReadBases, 
            int numberOfBlocks, 
            int checksum,
            List<int> slicePositions)
        {
            ContainerLength = containerLength;
            ReferenceSequenceId = referenceSequenceId;
            ReferenceStartingPosition = referenceStartingPosition;
            AlignmentSpan = alignmentSpan;
            NumberOfRecords = numberOfRecords;
            RecordCounter = recordCounter;
            NumberOfReadBases = numberOfReadBases;
            NumberOfBlocks = numberOfBlocks;
            Checksum = checksum;
            SlicePositions = slicePositions;
        }
    }
}