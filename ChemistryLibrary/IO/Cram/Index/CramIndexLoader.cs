using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Index
{
    public class CramIndexLoader
    {
        public CramIndex Load(string filePath)
        {
            using var fileStream = File.OpenRead(filePath);
            using var decompressedStream = new MemoryStream();
            GZip.Decompress(fileStream, decompressedStream, false);
            decompressedStream.Seek(0, SeekOrigin.Begin);

            using var stringReader = new StreamReader(decompressedStream);
            var indexEntries = new List<CramIndexEntry>();
            string line;
            while ((line = stringReader.ReadLine()) != null)
            {
                var splittedLine = line.Split('\t');
                if(splittedLine.Length != 6)
                    continue;
                var referenceSequenceId = int.Parse(splittedLine[0]);
                var alignmentStart = int.Parse(splittedLine[1]);
                var alignmentSpan = int.Parse(splittedLine[2]);
                var absoluteContainerOffset = long.Parse(splittedLine[3]);
                var relativeSliceHeaderOffset = int.Parse(splittedLine[4]);
                var sliceSize = int.Parse(splittedLine[5]);
                var indexEntry = new CramIndexEntry(
                    referenceSequenceId,
                    alignmentStart,
                    alignmentSpan,
                    absoluteContainerOffset,
                    relativeSliceHeaderOffset,
                    sliceSize);
                indexEntries.Add(indexEntry);
            }

            return new CramIndex(indexEntries);
        }
    }
}
