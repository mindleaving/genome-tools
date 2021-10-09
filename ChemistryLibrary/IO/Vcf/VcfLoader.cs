using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfLoader
    {
        public VcfLoaderResult Load(string filePath, Action<VcfVariantEntry, List<VcfMetadataEntry>> variantAction = null)
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
                if(variantAction != null)
                    variantAction(variant, metadataEntries);
                else
                    variants.Add(variant);
            }

            return new VcfLoaderResult(metadataEntries, variants);
        }

        private VcfMetadataEntry ParseMetadata(string line)
        {
            var splittedLine = line.Substring(2).Split('=', 2);
            if (splittedLine.Length != 2)
                throw new FormatException("Metadata must have the format '##TYPE=value'");
            var metadataType = splittedLine[0];
            var value = splittedLine[1];
            switch (metadataType)
            {
                case "fileformat":
                    //if (value != "VCFv4.3")
                    //    throw new NotSupportedException("Only VCF version 4.3 is supported");
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
                .Select(kvp => kvp.Split('=', 2))
                .Where(kvp => kvp.Length == 2) // Workaround for invalid VCF files with commas in tag values
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
            var otherFields = new Dictionary<string, string>();
            for (int i = 8; i < splittedLine.Length; i++)
            {
                var field = splittedLine[i];
                var columnName = header.Columns[i];
                otherFields.Add(columnName, field);
            }
            return new VcfVariantEntry(
                chromosome,
                position,
                id,
                referenceBases,
                alternateBases,
                quality,
                filterResult,
                info,
                otherFields);
        }

        private Dictionary<string, string> ParseVariantInfo(string infoString)
        {
            if (infoString == ".")
                return new Dictionary<string, string>();
            var keyValuePairs = infoString.Split(';')
                .Select(kvp => kvp.Split('=', 2))
                .Where(kvp => kvp.Length == 2) // Workaround for invalid VCF files with commas in tag values
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
}
