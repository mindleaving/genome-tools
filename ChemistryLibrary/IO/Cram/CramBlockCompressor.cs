using System;
using System.IO;
using GenomeTools.ChemistryLibrary.IO.Cram.Models;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramBlockCompressor
    {
        public byte[] Compress(byte[] uncompressedData, CramBlockHeader blockHeader)
        {
            switch (blockHeader.CompressionMethod)
            {
                case CramBlock.CompressionMethod.Raw:
                    return uncompressedData;
                case CramBlock.CompressionMethod.Gzip:
                {
                    using var instream = new MemoryStream(uncompressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    GZip.Compress(instream, outstream, false);
                    return outstream.ToArray();
                }
                case CramBlock.CompressionMethod.Bzip2:
                {
                    using var instream = new MemoryStream(uncompressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    BZip2.Compress(instream, outstream, false, 6);
                    return outstream.ToArray();
                }
                case CramBlock.CompressionMethod.Lzma:
                    break;
                case CramBlock.CompressionMethod.Rans:
                {
                    using var instream = new MemoryStream(uncompressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    RansDecoder.Encode(instream, outstream);
                    var compressBlockData = outstream.ToArray();
                    return compressBlockData;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }

        public byte[] Decompress(byte[] compressedData, CramBlockHeader blockHeader)
        {
            if (compressedData.Length == 0)
                return Array.Empty<byte>();
            switch (blockHeader.CompressionMethod)
            {
                case CramBlock.CompressionMethod.Raw:
                    return compressedData;
                case CramBlock.CompressionMethod.Gzip:
                {
                    using var instream = new MemoryStream(compressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    GZip.Decompress(instream, outstream, false);
                    return outstream.ToArray();
                }
                case CramBlock.CompressionMethod.Bzip2:
                {
                    using var instream = new MemoryStream(compressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    BZip2.Decompress(instream, outstream, false);
                    return outstream.ToArray();
                }
                case CramBlock.CompressionMethod.Lzma:
                    break;
                case CramBlock.CompressionMethod.Rans:
                {
                    using var instream = new MemoryStream(compressedData);
                    using var outstream = new MemoryStream(new byte[blockHeader.UncompressedSize]);
                    RansDecoder.Decode(instream, outstream);
                    var uncompressBlockData = outstream.ToArray();
                    return uncompressBlockData;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotImplementedException();
        }
    }
}
