using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Commons.Extensions;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class SamHeaderParser
    {
        public SamHeaderEntry Parse(string line)
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
    }
}