using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Commons.Extensions;
using GenomeTools.ChemistryLibrary.Genomics;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    public class VcfAccessor
    {
        private readonly string filePath;
        private List<VcfIndexEntry> indexEntries;
        private readonly Dictionary<string, string> sequenceNameTranslation;

        public VcfAccessor(string filePath,
            Dictionary<string,string> sequenceNameTranslation = null)
        {
            this.filePath = filePath;
            this.sequenceNameTranslation = sequenceNameTranslation;
        }

        public VcfLoaderResult LoadInRange(
            GenePosition genePosition, 
            Action<VcfVariantEntry,List<VcfMetadataEntry>> variantAction = null)
        {
            if (indexEntries == null) 
                LoadIndex();
            var chromosomeIndexEntries = indexEntries.Where(x => x.Chromosome == genePosition.Chromosome).ToList();
            if (!chromosomeIndexEntries.Any())
                throw new KeyNotFoundException($"Variant file doesn't contain variants for chromosome '{genePosition.Chromosome}'");
            var vcfIndexEntryBeforeGenePosition = chromosomeIndexEntries
                .Where(x => x.Position <= genePosition.Position.From)
                .ToList();
            var fileOffset = vcfIndexEntryBeforeGenePosition.Count > 0
                ? vcfIndexEntryBeforeGenePosition.MaximumItem(x => x.Position).FileOffset
                : chromosomeIndexEntries.First().FileOffset;
            return Load(
                variantAction,
                fileOffset: fileOffset,
                variantFilter: x => x.Chromosome == genePosition.Chromosome && genePosition.Position.Contains(x.Position),
                stopCriteria: x => x.Chromosome != genePosition.Chromosome || x.Position > genePosition.Position.To);
        }

        private void LoadIndex()
        {
            var vcfIndexReader = new VcfIndexReader(filePath + ".vcfi", sequenceNameTranslation);
            indexEntries = vcfIndexReader.Read();
        }

        public VcfLoaderResult Load(
            Action<VcfVariantEntry, List<VcfMetadataEntry>> variantAction = null,
            long fileOffset = -1,
            Func<VcfVariantEntry, bool> variantFilter = null,
            Func<VcfVariantEntry, bool> stopCriteria = null)
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8, false, 128*1000);
            var metadataEntries = new List<VcfMetadataEntry>();
            VcfHeader header = null;
            var variants = new List<VcfVariantEntry>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if(string.IsNullOrWhiteSpace(line))
                    continue;
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
                    if (fileOffset > 0)
                    {
                        reader.DiscardBufferedData();
                        reader.BaseStream.Seek(fileOffset, SeekOrigin.Begin);
                    }
                    continue;
                }

                if (header == null)
                    throw new FormatException("No header was found before data entries. Make sure there is a single header line starting with '#'");
                var variant = ParseVariant(line, header, sequenceNameTranslation);
                if(stopCriteria != null && stopCriteria(variant))
                    break;
                if(variantFilter != null && !variantFilter(variant))
                    continue;
                if(variantAction != null)
                    variantAction(variant, metadataEntries);
                else
                    variants.Add(variant);
            }

            return new VcfLoaderResult(metadataEntries, variants);
        }

        public VcfHeader ReadHeader()
        {
            var headerLine = File.ReadLines(filePath).SkipWhile(x => x.StartsWith("##")).First(x => x.StartsWith("#"));
            return ParseHeader(headerLine);
        }

        private static VcfMetadataEntry ParseMetadata(string line)
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

        private static VcfMetadataEntry ParseGenericMetadata(VcfMetadataEntry.MetadataType metadataType, string metadataTypeString, string value)
        {
            if(value.StartsWith("<") && value.EndsWith(">"))
            {
                var keyValuePairs = ParseMetadataKeyValuePairs(value);
                return new GenericVcfMetadataEntry(metadataType, metadataTypeString, keyValuePairs);
            }
            return new GenericVcfMetadataEntry(metadataType, metadataTypeString, value);
        }

        private static VcfMetadataEntry ParseContigMetadata(string value)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(value);
            var id = keyValuePairs["ID"];
            keyValuePairs.TryGetValue("length", out var length);
            keyValuePairs.TryGetValue("URL", out var url);
            return new ContigVcfMetadataEntry(id, length, url);
        }

        private static VcfMetadataEntry ParseAltMetadata(string value)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(value);
            var id = keyValuePairs["ID"];
            var description = keyValuePairs["Description"];
            return new AltVcfMetadataEntry(id, description);
        }

        private static FormatVcfMetadataEntry ParseFormatMetadata(string value)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(value);
            var id = keyValuePairs["ID"];
            if (!int.TryParse(keyValuePairs["Number"], out var number))
                number = -1;
            var valueType = keyValuePairs["Type"];
            var description = keyValuePairs["Description"];
            return new FormatVcfMetadataEntry(id, number, valueType, description);
        }

        private static FilterVcfMetadataEntry ParseFilterMetadata(string filter)
        {
            var keyValuePairs = ParseMetadataKeyValuePairs(filter);
            var id = keyValuePairs["ID"];
            var description = keyValuePairs["Description"];
            return new FilterVcfMetadataEntry(id, description);
        }

        private static InfoVcfMetadataEntry ParseInfoMetadata(string info)
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

        private static Dictionary<string,string> ParseMetadataKeyValuePairs(string value)
        {
            if (!value.StartsWith("<") || !value.EndsWith(">"))
                throw new FormatException("Invalid metadata key-value-pair format. Must have the format <key1=value1,key2=value2>");
            return value.Substring(1, value.Length-2)
                .Split(',')
                .Select(kvp => kvp.Split('=', 2))
                .Where(kvp => kvp.Length == 2) // Workaround for invalid VCF files with commas in tag values
                .ToDictionary(kvp => kvp[0], kvp => kvp[1]);
        }

        public static VcfHeader ParseHeader(string line)
        {
            var columns = line.Substring(1).Split('\t');
            return new VcfHeader(columns);
        }

        public static VcfVariantEntry ParseVariant(string line, VcfHeader header, Dictionary<string,string> sequenceNameTranslation = null)
        {
            var splittedLine = line.Split('\t');
            if (splittedLine.Length < 8)
                throw new FormatException($"A variant-line must contain at least 8 values delimited by a TAB, but line had only {splittedLine.Length} values");
            var chromosome = splittedLine[0];
            if(sequenceNameTranslation != null && sequenceNameTranslation.ContainsKey(chromosome))
                chromosome = sequenceNameTranslation[chromosome];
            var position = int.Parse(splittedLine[1]);
            var id = splittedLine[2];
            var referenceBases = splittedLine[3];
            var alternateBases = splittedLine[4].Split(',');
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

        private static Dictionary<string, string> ParseVariantInfo(string infoString)
        {
            if (infoString == ".")
                return new Dictionary<string, string>();
            var keyValuePairs = infoString.Split(';')
                .Select(kvp => kvp.Split('=', 2))
                .Where(kvp => kvp.Length == 2) // Workaround for invalid VCF files with commas in tag values
                .ToDictionary(kvp => kvp[0], kvp => kvp[1]);
            return keyValuePairs;
        }

        private static VcfFilterResult ParseFilterResult(string filterResultString)
        {
            if(filterResultString.ToLower() == "pass")
                return VcfFilterResult.Success();
            if (filterResultString == ".")
                return null;
            return VcfFilterResult.Failed(filterResultString.Split(','));
        }
    }
}
