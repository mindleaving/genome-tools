using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfLoader
    {
        public VcfLoaderResult Load(string filePath)
        {
            using var reader = new StreamReader(filePath);
            var metadataEntries = new List<VcfMetadataEntry>();
            VcfHeader header = null;
            var variants = new List<VcfVariantEntry>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var isMetadata = line.StartsWith("##");
                if (isMetadata)
                {
                    var metadata = ParseMetadata(line);
                    metadataEntries.Add(metadata);
                    continue;
                }

                var isHeader = line.StartsWith("#");
                if (isHeader)
                {
                    header = ParseHeader(line);
                    continue;
                }

                if (header == null)
                    throw new FormatException("No header was found before data entries. Make sure there is a single header line starting with '#'");
                var variant = ParseVariant(line, header);
                variants.Add(variant);
            }

            return new VcfLoaderResult(metadataEntries, variants);
        }

        private VcfMetadataEntry ParseMetadata(string line)
        {
            var splittedLine = line.Substring(2).Split(new []{'='}, 2);
            if (splittedLine.Length != 2)
                throw new FormatException("Metadata must have the format '##TYPE=value'");
            var metadataType = splittedLine[0];
            var value = splittedLine[1];
            switch (metadataType)
            {
                case "fileformat":
                    if (value != "VCFv4.3")
                        throw new NotSupportedException("Only VCF version 4.3 is supported");
                    return new FileFormatVcfMetadata(value);
                case "INFO":
                    return ParseInfoMetadata(value);
                case "FILTER":
                    return ParseFilterMetadata(value);
                case "FORMAT":
                    return ParseFormatMetadata(value);
                case "ALT":
                    return ParseAltMetadata(value);
                case "assembly":
                    return new AssemblyVcfMetadataEntry(value);
                case "contig":
                    return ParseContigMetadata(value);
                case "SAMPLE":
                    return ParseGenericMetadata(VcfMetadataEntry.MetadataType.Sample, metadataType, value);
                case "PEDIGREE":
                    return ParseGenericMetadata(VcfMetadataEntry.MetadataType.Pedigree, metadataType, value);
                default:
                    return ParseGenericMetadata(VcfMetadataEntry.MetadataType.Custom, metadataType, value);
            }
        }

        private VcfMetadataEntry ParseGenericMetadata(VcfMetadataEntry.MetadataType metadataType, string metadataTypeString, string value)
        {
            if(value.StartsWith("<") && value.EndsWith(">"))
            {
                var keyValuePairs = ParseMetadataKeyValuePairs(value);
                return new GenericVcfMetadataEntry(metadataType, metadataTypeString, keyValuePairs);
            }
            return new GenericVcfMetadataEntry(metadataType, metadataTypeString, value);
        }

        private VcfMetadataEntry ParseContigMetadata(string value)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(value);
            var id = keyValuePairs["ID"];
            keyValuePairs.TryGetValue("length", out var length);
            keyValuePairs.TryGetValue("URL", out var url);
            return new ContigVcfMetadataEntry(id, length, url);
        }

        private VcfMetadataEntry ParseAltMetadata(string value)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(value);
            var id = keyValuePairs["ID"];
            var description = keyValuePairs["Description"];
            return new AltVcfMetadataEntry(id, description);
        }

        private FormatVcfMetadataEntry ParseFormatMetadata(string value)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(value);
            var id = keyValuePairs["ID"];
            if (!int.TryParse(keyValuePairs["Number"], out var number))
                number = -1;
            var valueType = keyValuePairs["Type"];
            var description = keyValuePairs["Description"];
            return new FormatVcfMetadataEntry(id, number, valueType, description);
        }

        private FilterVcfMetadataEntry ParseFilterMetadata(string filter)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(filter);
            var id = keyValuePairs["ID"];
            var description = keyValuePairs["Description"];
            return new FilterVcfMetadataEntry(id, description);
        }

        private InfoVcfMetadataEntry ParseInfoMetadata(string info)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(info);
            var id = keyValuePairs["ID"];
            if (!int.TryParse(keyValuePairs["Number"], out var number))
                number = -1;
            var type = keyValuePairs["Type"];
            var description = keyValuePairs["Description"];
            return new InfoVcfMetadataEntry(
                id,
                number,
                type,
                description,
                keyValuePairs);
        }

        private Dictionary<string,string> ParseMetadataKeyValuePairs(string value)
        {
            if (!value.StartsWith("<") || !value.EndsWith(">"))
                throw new FormatException("Invalid metadata key-value-pair format. Must have the format <key1=value1,key2=value2>");
            return value.Substring(1, value.Length-2)
                .Split(',')
                .Select(kvp => kvp.Split(new []{'='}, 2))
                .ToDictionary(kvp => kvp[0], kvp => kvp[1]);
        }

        private VcfHeader ParseHeader(string line)
        {
            var columns = line.Substring(1).Split('\t');
            return new VcfHeader(columns);
        }

        private VcfVariantEntry ParseVariant(string line, VcfHeader header)
        {
            var splittedLine = line.Split('\t');
            if (splittedLine.Length < 8)
                throw new FormatException($"A variant-line must contain at least 8 values delimited by a TAB, but line had only {splittedLine.Length} values");
            var chromosome = splittedLine[0];
            var position = splittedLine[1];
            var id = splittedLine[2];
            var referenceBases = splittedLine[3];
            var alternateBases = splittedLine[4];
            var quality = splittedLine[5];
            var filterResult = ParseFilterResult(splittedLine[6]);
            var info = ParseVariantInfo(splittedLine[7]);
            return new VcfVariantEntry(
                chromosome,
                position,
                id,
                referenceBases,
                alternateBases,
                quality,
                filterResult,
                info);
        }

        private Dictionary<string, string> ParseVariantInfo(string infoString)
        {
            if (infoString == ".")
                return new Dictionary<string, string>();
            var keyValuePairs = infoString.Split(';')
                .Select(kvp => kvp.Split(new []{'='}, 2))
                .ToDictionary(kvp => kvp[0], kvp => kvp[1]);
            return keyValuePairs;
        }

        private VcfFilterResult ParseFilterResult(string filterResultString)
        {
            if(filterResultString.ToLower() == "pass")
                return VcfFilterResult.Success();
            if (filterResultString == ".")
                return null;
            return VcfFilterResult.Failed(filterResultString.Split(','));
        }
    }

    public class GenericVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType { get; }
        public string MetadataTypeString { get; }
        public Dictionary<string, string> KeyValuePairs { get; }
        public string Value { get; }

        public GenericVcfMetadataEntry(MetadataType metadataType, string metadataTypeString, Dictionary<string, string> keyValuePairs)
        {
            EntryType = metadataType;
            MetadataTypeString = metadataTypeString;
            KeyValuePairs = keyValuePairs;
        }

        public GenericVcfMetadataEntry(MetadataType metadataType, string metadataTypeString, string value)
        {
            EntryType = metadataType;
            MetadataTypeString = metadataTypeString;
            Value = value;
        }
    }

    public class ContigVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.Contig;
        public string Id { get; }
        public string Length { get; }
        public string Value { get; }

        public ContigVcfMetadataEntry(string id, string length, string value)
        {
            Id = id;
            Length = length;
            Value = value;
        }
    }

    public class FileFormatVcfMetadata : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.FileFormat;
        public string FileFormat { get; }

        public FileFormatVcfMetadata(string fileFormat)
        {
            FileFormat = fileFormat;
        }
    }

    public class AltVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.Alt;
        public string Id { get; }
        public string Description { get; }

        public AltVcfMetadataEntry(string id, string description)
        {
            Id = id;
            Description = description;
        }
    }

    public class FormatVcfMetadataEntry : VcfMetadataEntry
    {
        public string Id { get; }
        public int Number { get; }
        public string ValueType { get; }
        public string Description { get; }

        public FormatVcfMetadataEntry(
            string id, int number, string valueType,
            string description)
        {
            Id = id;
            Number = number;
            ValueType = valueType;
            Description = description;
        }

        public override MetadataType EntryType => MetadataType.Format;
    }

    public class FilterVcfMetadataEntry : VcfMetadataEntry
    {
        public override MetadataType EntryType => MetadataType.Filter;
        public string Id { get; }
        public string Description { get; }

        public FilterVcfMetadataEntry(string id, string description)
        {
            Id = id;
            Description = description;
        }
    }

    public class VcfHeader
    {
        public IList<string> Columns { get; }

        public VcfHeader(IList<string> columns)
        {
            Columns = columns;
        }
    }

    public class VcfVariantEntry
    {
        public string Chromosome { get; }
        public string Position { get; }
        public string Id { get; }
        public string ReferenceBases { get; }
        public string AlternateBases { get; }
        public string Quality { get; }
        public VcfFilterResult FilterResult { get; }
        public Dictionary<string,string> Info { get; }

        public VcfVariantEntry(
            string chromosome, string position, string id,
            string referenceBases, string alternateBases, string quality,
            VcfFilterResult filterResult, 
            Dictionary<string, string> info)
        {
            Chromosome = chromosome;
            Position = position;
            Id = id;
            ReferenceBases = referenceBases;
            AlternateBases = alternateBases;
            Quality = quality;
            FilterResult = filterResult;
            Info = info;
        }
    }

    public class VcfFilterResult
    {
        public bool Pass { get; }
        /// <summary>
        /// Null if <see cref="Pass"/> is true
        /// </summary>
        public IList<string> FailingFilters { get; }

        private VcfFilterResult(bool pass, IList<string> failingFilters)
        {
            Pass = pass;
            FailingFilters = failingFilters;
        }

        public static VcfFilterResult Success()
        {
            return new VcfFilterResult(true, null);
        }

        public static VcfFilterResult Failed(IList<string> failingFilters)
        {
            return new VcfFilterResult(false, failingFilters);
        }
    }

    public abstract class VcfMetadataEntry
    {
        public enum MetadataType
        {
            FileFormat,
            Info,
            Format,
            Filter,
            Alt,
            Assembly,
            Contig,
            Sample,
            Pedigree,
            Custom
        }
        public abstract MetadataType EntryType { get; }
    }

    public class AssemblyVcfMetadataEntry : VcfMetadataEntry
    {
        public AssemblyVcfMetadataEntry(string url)
        {
            Url = url;
        }

        public override MetadataType EntryType => MetadataType.Assembly;
        public string Url { get; }
    }

    public class VcfLoaderResult
    {
        public List<VcfMetadataEntry> MetadataEntries { get; }
        public List<VcfVariantEntry> Variants { get; }

        public VcfLoaderResult(List<VcfMetadataEntry> metadataEntries, List<VcfVariantEntry> variants)
        {
            MetadataEntries = metadataEntries;
            Variants = variants;
        }
    }
}
