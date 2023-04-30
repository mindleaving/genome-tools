using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GenomeTools.ChemistryLibrary.IO.Cram.Index;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramSliceReader
    {
        /// <summary>
        /// Reads a slice, which contains one or more records
        /// </summary>
        /// <param name="reader">Reader</param>
        /// <param name="sliceRelativeOffset">Slice byte offset relative to first block of this container</param>
        /// <param name="firstBlockAbsoluteOffset">Absolute byte offset of first block</param>
        /// /// <param name="containerHeader">Header of container in which the slice is positioned</param>
        /// <param name="compressionHeader">Compression header</param>
        public CramSlice Read(
            CramBinaryReader reader, 
            int sliceRelativeOffset, 
            long firstBlockAbsoluteOffset,
            CramContainerHeader containerHeader,
            CramCompressionHeader compressionHeader)
        {
            if (reader.Position != firstBlockAbsoluteOffset + sliceRelativeOffset)
                reader.Seek(firstBlockAbsoluteOffset + sliceRelativeOffset, SeekOrigin.Begin);
            var sliceHeader = ReadSliceHeader(reader);
            var blockReader = new CramBlockReader();
            var coreDataBlock = blockReader.Read(reader, compressionHeader);
            var externalDataBlock = blockReader.ReadBlocks(reader, sliceHeader.NumberOfBlocks - 1, compressionHeader);
            return new CramSlice(containerHeader, compressionHeader, sliceHeader, coreDataBlock, externalDataBlock);
        }

        /// <summary>
        /// Read slice corresponding to index entry
        /// </summary>
        public CramSlice Read(CramBinaryReader reader, CramIndexEntry indexEntry)
        {
            var containerHeaderReader = new CramContainerHeaderReader();
            var containerHeader = containerHeaderReader.Read(reader, indexEntry.AbsoluteContainerOffset);
            var sliceOffsetReference = reader.Position;

            var compressionHeaderReader = new CramCompressionHeaderReader();
            var compressionHeader = compressionHeaderReader.Read(reader);

            return Read(
                reader,
                indexEntry.RelativeSliceHeaderOffset,
                sliceOffsetReference,
                containerHeader,
                compressionHeader);
        }

        public List<CramSlice> ReadMany(
            CramBinaryReader reader, 
            long sliceOffset, 
            CramContainerHeader containerHeader, 
            CramCompressionHeader compressionHeader)
        {
            return containerHeader.SlicePositions
                .Select(slicePosition => Read(reader, slicePosition, sliceOffset, containerHeader, compressionHeader))
                .ToList();
        }


        private CramSliceHeader ReadSliceHeader(CramBinaryReader reader)
        {
            // Block header
            var blockHeaderReader = new CramBlockHeaderReader();
            var blockHeader = blockHeaderReader.Read(reader);
            if (blockHeader.CompressionMethod != CramBlock.CompressionMethod.Raw)
                throw new NotSupportedException("Found a compressed slice header. Slice headers must not be compressed");

            // Block data
            var blockDataStartPosition = reader.Position;
            var referenceSequenceId = reader.ReadItf8();
            var alignmentStart = reader.ReadItf8();
            var alignmentSpan = reader.ReadItf8();
            var numberOfRecords = reader.ReadItf8();
            var recordCounter = reader.ReadLtf8();
            var numberOfBlocks = reader.ReadItf8();
            var blockContentIds = reader.ReadCramItf8Array();
            var embeddedReferenceBlockContentId = reader.ReadItf8();
            var referenceMd5Checksum = reader.ReadBytes(16);

            var blockDataEndPosition = blockDataStartPosition + blockHeader.UncompressedSize; // True because slice headers are always uncompressed
            var tags = ReadSliceTags(reader, blockDataEndPosition);

            // Block checksum
            var blockChecksum = reader.ReadInt32();

            return new CramSliceHeader(
                referenceSequenceId,
                alignmentStart,
                alignmentSpan,
                numberOfRecords,
                recordCounter,
                numberOfBlocks,
                blockContentIds,
                embeddedReferenceBlockContentId,
                referenceMd5Checksum,
                tags);
        }

        private Dictionary<string, object> ReadSliceTags(CramBinaryReader reader, long blockDataEndPosition)
        {
            var tags = new Dictionary<string, object>();
            var tagReader = new CramTagReader();
            while (reader.Position < blockDataEndPosition)
            {
                var tag = Encoding.ASCII.GetString(reader.ReadBytes(2));
                var tagValue = tagReader.ReadTagValue(reader);
                tags.Add(tag, tagValue);
            }
            return tags;
        }
    }
}
