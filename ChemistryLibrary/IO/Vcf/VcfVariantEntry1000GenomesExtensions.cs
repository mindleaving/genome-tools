using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GenomeTools.ChemistryLibrary.IO.Vcf;

public static class VcfVariantEntry1000GenomesExtensions
{
    public static List<string> GetGenomeIds(
        this VcfVariantEntry variantEntry)
        => variantEntry.OtherFields.Keys.Skip(1).ToList(); // Skip 1 for FORMAT column

    public static bool HasPersonVariant(
        this VcfVariantEntry variantEntry,
        string personId)
    {
        if (!variantEntry.OtherFields.ContainsKey(personId))
            return false;
        var variantFlag = variantEntry.OtherFields[personId];
        if (variantFlag == "1")
            return true;
        return variantFlag != "0|0" && Regex.IsMatch(variantFlag, "([1-9][0-9]*(/|\\|)[0-9]+|[0-9]+(/|\\|)[1-9][0-9]*)");
    }
}
public static class VcfVariantEntryExtensions
{
    public static bool IsHeterogenous(this VcfVariantEntry variant, string genomeId)
    {
        var fieldNames = variant.OtherFields["FORMAT"].Split(':').ToList();
        var genoTypeIndex = fieldNames.FindIndex(x => x == "GT");
        var splittedValues = variant.OtherFields[genomeId].Split(':');
        if (splittedValues.Length != fieldNames.Count)
            throw new Exception("Genotype and other information was in an unexpected format");
        var genoType = splittedValues[genoTypeIndex];
        var isPhased = genoType[1] == '|';
        var parent1HasVariant = genoType[0] == '1';
        var parent2HasVariant = genoType.Length == 3 && genoType[2] == '1';
        return parent1HasVariant != parent2HasVariant;
    }
}