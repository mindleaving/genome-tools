using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GenomeTools.ChemistryLibrary.Extensions;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class SamLoader
    {
        private readonly SamHeaderParser headerParser;

        public SamLoader()
        {
            headerParser = new SamHeaderParser();
        }

        public SamLoaderResult Load(
            string filePath)
        {
            return Load(File.OpenRead(filePath));
        }

        public SamLoaderResult Load(Stream stream)
        {
            using var reader = new StreamReader(stream);
            var headerEntries = new List<SamHeaderEntry>();
            var alignmentEntries = new List<SamAlignmentEntry>();
            var hasCheckedHeaders = false;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if(string.IsNullOrWhiteSpace(line))
                    continue;
                var isHeaderLine = line.StartsWith("@");
                if (isHeaderLine)
                {
                    var headerEntry = headerParser.Parse(line);
                    if (headerEntry.Type == SamHeaderEntry.HeaderEntryType.FileLevelMetadata && headerEntries.Any())
                        throw new FormatException("Detected a HD-header after the first line of the file, which is not allowed");
                    if (headerEntry.Type == SamHeaderEntry.HeaderEntryType.Program)
                    {
                        CheckUniqueIdForProgramHeaderEntry(headerEntry, headerEntries);
                    }
                    if (headerEntry.Type == SamHeaderEntry.HeaderEntryType.ReadGroup)
                    {
                        CheckUniqueIdForReadGroupHeaderEntry(headerEntry, headerEntries);
                    }
                    headerEntries.Add(headerEntry);
                }
                else
                {
                    if (!hasCheckedHeaders)
                    {
                        CheckHeaders(headerEntries);
                        hasCheckedHeaders = true;
                    }
                    var alignmentEntry = ParseAlignmentEntry(line);
                    alignmentEntries.Add(alignmentEntry);
                }
            }
            if (!hasCheckedHeaders) 
                CheckHeaders(headerEntries);

            return new SamLoaderResult(headerEntries, alignmentEntries);
        }

        private static void CheckUniqueIdForProgramHeaderEntry(SamHeaderEntry headerEntry, List<SamHeaderEntry> headerEntries)
        {
            var newProgramHeaderEntry = (ProgramSamHeaderEntry)headerEntry;
            var hasProgramHeaderWithSameId = headerEntries.Where(x => x.Type == SamHeaderEntry.HeaderEntryType.Program)
                .Cast<ProgramSamHeaderEntry>()
                .Any(x => x.ProgramId == newProgramHeaderEntry.ProgramId);
            if (hasProgramHeaderWithSameId)
                throw new FormatException("Detected two PG-headers with same ID");
        }

        private static void CheckUniqueIdForReadGroupHeaderEntry(SamHeaderEntry headerEntry, List<SamHeaderEntry> headerEntries)
        {
            var newReadGroupHeaderEntry = (ReadGroupSamHeaderEntry)headerEntry;
            var hasReadGroupHeaderWithSameId = headerEntries.Where(x => x.Type == SamHeaderEntry.HeaderEntryType.ReadGroup)
                .Cast<ReadGroupSamHeaderEntry>()
                .Any(x => x.ReadGroupId == newReadGroupHeaderEntry.ReadGroupId);
            if (hasReadGroupHeaderWithSameId)
                throw new FormatException("Detected two RG-headers with same ID");
        }

        private void CheckHeaders(List<SamHeaderEntry> headerEntries)
        {
            var programHeaderEntries = headerEntries
                .Where(x => x.Type == SamHeaderEntry.HeaderEntryType.Program)
                .Cast<ProgramSamHeaderEntry>()
                .ToList();
            foreach (var programSamHeaderEntry in programHeaderEntries)
            {
                if (!string.IsNullOrEmpty(programSamHeaderEntry.PreviousProgramId))
                {
                    var hasProgramHeaderWithReferencedId = programHeaderEntries.Any(x => x.ProgramId == programSamHeaderEntry.PreviousProgramId);
                    if(!hasProgramHeaderWithReferencedId)
                    {
                        throw new FormatException($"Could not find a PG-header with ID '{programSamHeaderEntry.PreviousProgramId}', "
                                                  + $"which was referenced by PG-header with ID '{programSamHeaderEntry.ProgramId}'");
                    }
                }
            }
        }

        private SamAlignmentEntry ParseAlignmentEntry(string line)
        {
            var splittedLine = line.Split('\t');
            var qname = splittedLine[0];
            var flag = (SamAlignmentFlag)uint.Parse(splittedLine[1]);
            var rname = splittedLine[2];
            var pos = int.Parse(splittedLine[3]);
            var mapq = int.Parse(splittedLine[4]);
            var cigar = splittedLine[5];
            var rnext = splittedLine[6];
            var pnext = int.Parse(splittedLine[7]);
            var tlen = int.Parse(splittedLine[8]);
            var seq = splittedLine[9];
            var qual = splittedLine[10];
            var optionalFields = new Dictionary<string, object>();
            for (int i = 11; i < splittedLine.Length; i++)
            {
                var field = splittedLine[i].Split(new[] { ':' }, 3);
                var tag = field[0];
                if (!Regex.IsMatch(tag, "^[A-Za-z][0-9A-Za-z]$"))
                    throw new FormatException($"Invalid tag '{tag}'");
                var type = field[1];
                var value = field[2];
                object parsedValue;
                switch (type)
                {
                    case "A":
                    {
                        if (!Regex.IsMatch(value, "^[!-~]$"))
                            throw new FormatException($"Invalid value '{value}' for tag of type '{type}'");
                        parsedValue = value;
                        break;
                    }
                    case "i":
                    {
                        parsedValue = int.Parse(value);
                        break;
                    }
                    case "f":
                    {
                        parsedValue = double.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    }
                    case "Z":
                    {
                        if (!Regex.IsMatch(value, "^[ !-~]*$"))
                            throw new FormatException($"Invalid value '{value}' for tag of type '{type}'");
                        parsedValue = value;
                        break;
                    }
                    case "H":
                    {
                        parsedValue = ParserHelpers.ParseHexString(value);
                        break;
                    }
                    case "B":
                    {
                        parsedValue = ParseArray(value);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), $"Unknown alignment optional field value type '{type}'. Supported: A, i, f, Z, H, B");
                }
                optionalFields.Add(tag, parsedValue);
            }
            return new SamAlignmentEntry(
                qname,
                flag,
                rname,
                pos,
                mapq,
                cigar,
                rnext,
                pnext,
                tlen,
                seq,
                qual,
                optionalFields);
        }

        private object ParseArray(string value)
        {
            var valueType = value[0];
            var splittedValues = value.Split(',').Skip(1);
            switch (valueType)
            {
                case 'c':
                case 's':
                case 'i':
                    return splittedValues.Select(int.Parse).ToList();
                case 'C':
                case 'S':
                case 'I':
                    return splittedValues.Select(uint.Parse).ToList();
                case 'f':
                    return splittedValues.Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(valueType), $"Unknown value type '{valueType}' for numeric array. Supported: c,C,s,S,i,I,f");
            }
        }

        private bool IsReferenceNameValid(string name)
        {
            return Regex.IsMatch(name, "^[0-9A-Za-z!#$%&+./:;?@^_|~-][0-9A-Za-z!#$%&*+./:;=?@^_|~-]*$");
        }

        private bool IsSubSortingOrderValid(string subsortingOrder)
        {
            return Regex.IsMatch(subsortingOrder, "^(coordinate|queryname|unsorted)(:[A-Za-z0-9_-]+)+$");
        }
    }
}
