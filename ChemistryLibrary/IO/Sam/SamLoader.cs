using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class SamLoader
    {
        public SamLoaderResult Load(string filePath)
        {
            using var reader = new StreamReader(filePath);
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
                    var headerEntry = ParseHeaderEntry(line);
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
                        parsedValue = ParseHexString(value);
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

        private byte[] ParseHexString(string hexString)
        {
            if(hexString.Length.IsOdd())
                throw new FormatException("A hex string must have an even number of characters in the range 0-9A-F (lower case is also acceptable) to be valid. "
                                          + $"Length was {hexString.Length}.");
            var result = new byte[hexString.Length / 2];
            for (int byteIndex = 0; byteIndex < result.Length; byteIndex++)
            {
                result[byteIndex] = Convert.ToByte(hexString.Substring(byteIndex * 2, 2), 16);
            }
            return result;
        }

        private SamHeaderEntry ParseHeaderEntry(string line)
        {
            var splittedLine = line.Split('\t');
            var headerType = splittedLine[0].Substring(1);
            var keyValuePairs = new Dictionary<string, string>();
            if (headerType != "CO")
            {
                for (int i = 1; i < splittedLine.Length; i++)
                {
                    var keyValuePair = splittedLine[i].Split(new [] {':'}, 2);
                    keyValuePairs.Add(keyValuePair[0], keyValuePair[1]);
                }
            }

            switch (headerType)
            {
                case "HD":
                {
                    var formatVersion = Version.Parse(keyValuePairs["VN"]);
                    keyValuePairs.TryGetValue("SO", out var alignmentSortingOrder);
                    if (alignmentSortingOrder != null)
                    {
                        if(!alignmentSortingOrder.ToLower().InSet("unknown", "unsorted", "queryname", "coordinate"))
                            throw new FormatException($"Invalid SO-tag in HD-header. Value '{alignmentSortingOrder}' is not supported");
                    }
                    keyValuePairs.TryGetValue("GO", out var groupingOfAlignment);
                    if (groupingOfAlignment != null)
                    {
                        if (!groupingOfAlignment.ToLower().InSet("none", "query", "reference"))
                            throw new FormatException($"Invalid GO-tag in HD-header. Value '{groupingOfAlignment}' is not supported");
                    }
                    keyValuePairs.TryGetValue("SS", out var subsortingOrderOfAlignments);
                    if (subsortingOrderOfAlignments != null)
                    {
                        if (alignmentSortingOrder != null && !subsortingOrderOfAlignments.ToLower().StartsWith(alignmentSortingOrder.ToLower()))
                            throw new FormatException($"Sub-sorting order (SS-tag in HD-header) must match SO-tag. SS-tag: {subsortingOrderOfAlignments}, SO-tag: {alignmentSortingOrder}");
                        if (!Regex.IsMatch(subsortingOrderOfAlignments, "^(coordinate|queryname|unsorted)(:[A-Za-z0-9_-]+)+$"))
                            throw new FormatException($"Invalid SS-tag value in HD-header. Value: '{subsortingOrderOfAlignments}'");
                    }
                    return new FileLevelMetadataSamHeaderEntry(
                        formatVersion,
                        alignmentSortingOrder,
                        groupingOfAlignment,
                        subsortingOrderOfAlignments);
                }
                case "SQ":
                {
                    var referenceSequenceName = keyValuePairs["SN"];
                    var referenceSequenceLength = uint.Parse(keyValuePairs["LN"]);
                    keyValuePairs.TryGetValue("AH", out var alternativeLocus);
                    keyValuePairs.TryGetValue("AN", out var alternativeReferenceNames);
                    keyValuePairs.TryGetValue("AS", out var genomeAssemblyId);
                    keyValuePairs.TryGetValue("DS", out var description);
                    keyValuePairs.TryGetValue("M5", out var md5Checksum);
                    keyValuePairs.TryGetValue("SP", out var species);
                    keyValuePairs.TryGetValue("TP", out var moleculeTopology);
                    keyValuePairs.TryGetValue("UR", out var storageLocation);
                    return new ReferenceSequenceSamHeaderEntry(
                        referenceSequenceName,
                        referenceSequenceLength,
                        alternativeLocus,
                        alternativeReferenceNames?.Split(',').ToList(),
                        genomeAssemblyId,
                        description,
                        md5Checksum,
                        species,
                        moleculeTopology,
                        storageLocation);
                }
                case "RG":
                {
                    var readGroupId = keyValuePairs["ID"];
                    keyValuePairs.TryGetValue("BC", out var barcode);
                    keyValuePairs.TryGetValue("CN", out var sequencingCenter);
                    keyValuePairs.TryGetValue("DS", out var description);
                    keyValuePairs.TryGetValue("DT", out var dateString);
                    DateTime? date = dateString != null ? DateTime.Parse(dateString) : null;
                    keyValuePairs.TryGetValue("FO", out var flowOrder);
                    keyValuePairs.TryGetValue("KS", out var keySequence);
                    keyValuePairs.TryGetValue("LB", out var library);
                    keyValuePairs.TryGetValue("PG", out var programs);
                    keyValuePairs.TryGetValue("PI", out var predictedMedianInsertSize);
                    keyValuePairs.TryGetValue("PL", out var platform);
                    keyValuePairs.TryGetValue("PM", out var platformModel);
                    keyValuePairs.TryGetValue("PU", out var platformUnit);
                    keyValuePairs.TryGetValue("SM", out var sample);
                    return new ReadGroupSamHeaderEntry(
                        readGroupId,
                        barcode,
                        sequencingCenter,
                        description,
                        date,
                        flowOrder,
                        keySequence,
                        library,
                        programs,
                        predictedMedianInsertSize,
                        platform,
                        platformModel,
                        platformUnit,
                        sample);
                }
                case "PG":
                {
                    var programId = keyValuePairs["ID"];
                    keyValuePairs.TryGetValue("PN", out var programName);
                    keyValuePairs.TryGetValue("CL", out var commandLine);
                    keyValuePairs.TryGetValue("PP", out var previousProgramId);
                    keyValuePairs.TryGetValue("DS", out var description);
                    keyValuePairs.TryGetValue("VN", out var programVersion);
                    return new ProgramSamHeaderEntry(programId, programName, commandLine, previousProgramId, description, programVersion);
                }
                case "CO":
                {
                    var comment = splittedLine[1];
                    return new CommentSamHeaderEntry(comment);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(headerType), $"Unknown SAM header type '{headerType}'");
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

    public class FileLevelMetadataSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.FileLevelMetadata;
        public Version FormatVersion { get; }
        public string AlignmentSortingOrder { get; }
        public string GroupingOfAlignment { get; }
        public string SubsortingOrderOfAlignments { get; }
        
        public FileLevelMetadataSamHeaderEntry(
            Version formatVersion, string alignmentSortingOrder, string groupingOfAlignment,
            string subsortingOrderOfAlignments)
        {
            FormatVersion = formatVersion;
            AlignmentSortingOrder = alignmentSortingOrder;
            GroupingOfAlignment = groupingOfAlignment;
            SubsortingOrderOfAlignments = subsortingOrderOfAlignments;
        }
    }

    public class ReferenceSequenceSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.ReferenceSequence;
        public string ReferenceSequenceName { get; }
        public uint ReferenceSequenceLength { get; }
        public string AlternativeLocus { get; }
        public List<string> AlternativeNames { get; }
        public string GenomeAssemblyId { get; }
        public string Description { get; }
        public string Md5Checksum { get; }
        public string Species { get; }
        public string MoleculeTopology { get; }
        public string StorageLocation { get; }

        public ReferenceSequenceSamHeaderEntry(
            string referenceSequenceName, uint referenceSequenceLength, string alternativeLocus,
            List<string> alternativeNames, string genomeAssemblyId, string description,
            string md5Checksum, string species, string moleculeTopology,
            string storageLocation)
        {
            ReferenceSequenceName = referenceSequenceName;
            ReferenceSequenceLength = referenceSequenceLength;
            AlternativeLocus = alternativeLocus;
            AlternativeNames = alternativeNames ?? new List<string>();
            GenomeAssemblyId = genomeAssemblyId;
            Description = description;
            Md5Checksum = md5Checksum;
            Species = species;
            MoleculeTopology = moleculeTopology;
            StorageLocation = storageLocation;
        }
    }

    internal class ReadGroupSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.ReadGroup;
        public string ReadGroupId { get; }
        public string Barcode { get; }
        public string SequencingCenter { get; }
        public string Description { get; }
        public DateTime? Date { get; }
        public string FlowOrder { get; }
        public string KeySequence { get; }
        public string Library { get; }
        public string Programs { get; }
        public string PredictedMedianInsertSize { get; }
        public string Platform { get; }
        public string PlatformModel { get; }
        public string PlatformUnit { get; }
        public string Sample { get; }

        public ReadGroupSamHeaderEntry(
            string readGroupId, string barcode, string sequencingCenter,
            string description, DateTime? date, string flowOrder,
            string keySequence, string library, string programs,
            string predictedMedianInsertSize, string platform, string platformModel,
            string platformUnit, string sample)
        {
            ReadGroupId = readGroupId;
            Barcode = barcode;
            SequencingCenter = sequencingCenter;
            Description = description;
            Date = date;
            FlowOrder = flowOrder;
            KeySequence = keySequence;
            Library = library;
            Programs = programs;
            PredictedMedianInsertSize = predictedMedianInsertSize;
            Platform = platform;
            PlatformModel = platformModel;
            PlatformUnit = platformUnit;
            Sample = sample;
        }
    }

    public class ProgramSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.Program;
        public string ProgramId { get; }
        public string ProgramName { get; }
        public string CommandLine { get; }
        public string PreviousProgramId { get; }
        public string Description { get; }
        public string ProgramVersion { get; }

        public ProgramSamHeaderEntry(
            string programId, string programName, string commandLine,
            string previousProgramId, string description, string programVersion)
        {
            ProgramId = programId;
            ProgramName = programName;
            CommandLine = commandLine;
            PreviousProgramId = previousProgramId;
            Description = description;
            ProgramVersion = programVersion;
        }
    }

    public class CommentSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.Comment;
        public string Comment { get; }

        public CommentSamHeaderEntry(string comment)
        {
            Comment = comment;
        }
    }

    [Flags]
    public enum SamAlignmentFlag : uint
    {
        TemplateMultipleSegments = 1 << 0,
        ProperlyAligned = 1 << 1,
        Unmapped = 1 << 2,
        NextSegmentUnmapped = 1 << 3,
        SeqReverseComplement = 1 << 4,
        SeqNextSegmentReverseComplement = 1 << 5,
        FirstSegment = 1 << 6,
        LastSegment = 1 << 7,
        SecondaryAlignment = 1 << 8,
        LowQuality = 1 << 9,
        PcrOrOpticalDuplicate = 1 << 10,
        SupplementaryAlignment = 1 << 11
    }
    public class SamAlignmentEntry
    {
        public string Qname { get; }
        public SamAlignmentFlag Flag { get; }
        public string Rname { get; }
        public int Pos { get; }
        public int Mapq { get; }
        public string Cigar { get; }
        public string Rnext { get; }
        public int Pnext { get; }
        public int Tlen { get; }
        public string Seq { get; }
        public string Qual { get; }
        public Dictionary<string,object> OptionalFields { get; }

        public SamAlignmentEntry(
            string qname, SamAlignmentFlag flag, string rname,
            int pos, int mapq, string cigar,
            string rnext, int pnext, int tlen,
            string seq, string qual, 
            Dictionary<string, object> optionalFields)
        {
            Qname = qname;
            Flag = flag;
            Rname = rname;
            Pos = pos;
            Mapq = mapq;
            Cigar = cigar;
            Rnext = rnext;
            Pnext = pnext;
            Tlen = tlen;
            Seq = seq;
            Qual = qual;
            OptionalFields = optionalFields ?? new Dictionary<string, object>();
        }
    }

    public abstract class SamHeaderEntry
    {
        public enum HeaderEntryType
        {
            FileLevelMetadata,
            ReferenceSequence,
            ReadGroup,
            Program,
            Comment
        }

        public abstract HeaderEntryType Type { get; }
    }

    public class SamLoaderResult
    {
        public List<SamHeaderEntry> HeaderEntries { get; }
        public List<SamAlignmentEntry> AlignmentEntries { get; }

        public SamLoaderResult(List<SamHeaderEntry> headerEntries, List<SamAlignmentEntry> alignmentEntries)
        {
            HeaderEntries = headerEntries;
            AlignmentEntries = alignmentEntries;
        }
    }
}
