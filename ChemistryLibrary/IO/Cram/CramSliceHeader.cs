using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramSliceHeader
    {
        public int ReferenceSequenceId { get; }
        public int AlignmentStart { get; }
        public int AlignmentSpan { get; }
        public int NumberOfRecords { get; }
        public long RecordCounter { get; }
        public int NumberOfBlocks { get; }
        public List<int> BlockContentIds { get; }
        public int EmbeddedReferenceBlockContentId { get; }
        public byte[] ReferenceMD5Checksum { get; }
        public Dictionary<string,object> Tags { get; }

        public CramSliceHeader(int referenceSequenceId, int alignmentStart, int alignmentSpan,
            int numberOfRecords, long recordCounter, int numberOfBlocks,
            List<int> blockContentIds, int embeddedReferenceBlockContentId, byte[] referenceMd5Checksum,
            Dictionary<string, object> tags)
        {
            ReferenceSequenceId = referenceSequenceId;
            AlignmentStart = alignmentStart;
            AlignmentSpan = alignmentSpan;
            NumberOfRecords = numberOfRecords;
            RecordCounter = recordCounter;
            NumberOfBlocks = numberOfBlocks;
            BlockContentIds = blockContentIds;
            EmbeddedReferenceBlockContentId = embeddedReferenceBlockContentId;
            ReferenceMD5Checksum = referenceMd5Checksum;
            Tags = tags;
        }
    }
}