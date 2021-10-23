using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GenomeTools.ChemistryLibrary.IO.Fasta
{
    public class FastaIndexBuilder
    {
        public List<FastaIndexEntry> BuildIndex(Stream fastaFileStream)
        {
            return BuildIndexImpl(fastaFileStream);
        }

        public void BuildIndexAndWriteToFile(string fastaFilePath, string outputDirectory = null)
        {
            if (outputDirectory != null && !Directory.Exists(outputDirectory))
                throw new DirectoryNotFoundException($"Directory {outputDirectory} doesn't exist");
            var outputFilePath = Path.Combine(outputDirectory ?? Path.GetDirectoryName(fastaFilePath) ?? ".", Path.GetFileName(fastaFilePath) + ".fai");
            var fastaFileStream = File.OpenRead(fastaFilePath);
            var outputStream = File.OpenWrite(outputFilePath);
            BuildIndexImpl(fastaFileStream, outputStream);
        }
        private List<FastaIndexEntry> BuildIndexImpl(Stream fastaFileStream, Stream outputStream = null, bool leaveStreamsOpen = false)
        {
            using var reader = new UnbufferedStreamReader(fastaFileStream, leaveStreamOpen: leaveStreamsOpen);
            TextWriter writer = outputStream != null ? new StreamWriter(outputStream, Encoding.UTF8, 1024, leaveStreamsOpen) : null;
            var indexEntries = outputStream != null ? null : new List<FastaIndexEntry>();
            FastaIndexEntry currentSection = null;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmedLine =  line.Trim();
                if (currentSection != null && (trimmedLine.StartsWith(">") || trimmedLine.Length < currentSection.BasesPerLine))
                {
                    if (line.Length < currentSection.BasesPerLine)
                        currentSection.Length += trimmedLine.Length;
                    if(writer != null)
                        WriteSectionToIndex(currentSection, writer);
                    indexEntries?.Add(currentSection);
                    currentSection = null;
                }
                if (trimmedLine.StartsWith(">"))
                {
                    var firstBaseOffset = reader.Position;
                    var sequenceId = GetSequenceIdFromHeader(trimmedLine);
                    line = reader.ReadLine();
                    trimmedLine = line.Trim();
                    var basesPerLine = (ushort)trimmedLine.Length;
                    var lineWidth = (ushort)(reader.Position - firstBaseOffset);
                    currentSection = new FastaIndexEntry(sequenceId, firstBaseOffset, basesPerLine, lineWidth);
                }
                if (currentSection != null)
                {
                    var lineLength = trimmedLine.Length;
                    if (lineLength > currentSection.BasesPerLine)
                    {
                        throw new FormatException($"Detected differing number of bases per line for sequence ID '{currentSection.SequenceName}' "
                                                  + $"at file offset {reader.Position}");
                    }
                    currentSection.Length += lineLength;
                }
            }
            if(currentSection != null)
            {
                if(writer != null)
                    WriteSectionToIndex(currentSection, writer);
                indexEntries?.Add(currentSection);
            }
            if(writer != null)
                writer.Dispose();
            return indexEntries;
        }

        private string GetSequenceIdFromHeader(string line)
        {
            var match = Regex.Match(line, @"^>(?<ID>[^\s]+)(\s(?<comment>.*))?$");
            var sequenceName = match.Groups["ID"].Value;
            return sequenceName;
        }

        private void WriteSectionToIndex(FastaIndexEntry section, TextWriter writer)
        {
            writer.WriteLine($"{section.SequenceName}\t{section.Length}\t{section.FirstBaseOffset}\t{section.BasesPerLine}\t{section.LineWidth}");
        }
    }
}
