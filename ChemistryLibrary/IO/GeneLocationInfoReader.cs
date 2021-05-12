using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenomeTools.ChemistryLibrary.Extensions;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.IO
{
    public static class GeneLocationInfoReader
    {
        public static List<GeneLocationInfo> ReadPeptides(string peptideDataFile)
        {
            var peptides = new List<GeneLocationInfo>();
            using (var streamReader = new StreamReader(peptideDataFile))
            {
                GeneLocationInfo geneLocationInfo = null;
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith(">"))
                    {
                        var splittedHeader = line.Split(':', ' ').ToList();
                        // ReSharper disable once InconsistentNaming
                        var GRCh38Index = splittedHeader.IndexOf("GRCh38");
                        var chromosome = splittedHeader[GRCh38Index + 1];
                        if (chromosome == "X" || chromosome == "Y" || chromosome.All(char.IsNumber))
                        {
                            geneLocationInfo = new GeneLocationInfo();
                            peptides.Add(geneLocationInfo);
                            var startIndex = int.Parse(splittedHeader[GRCh38Index + 2]);
                            var endIndex = int.Parse(splittedHeader[GRCh38Index + 3]);
                            geneLocationInfo.Chromosome = chromosome;
                            geneLocationInfo.StartBase = startIndex;
                            geneLocationInfo.EndBase = endIndex;

                            var geneSymbolIndex = splittedHeader.IndexOf("gene_symbol");
                            if (geneSymbolIndex >= 0)
                                geneLocationInfo.GeneSymbol = splittedHeader[geneSymbolIndex + 1];
                        }
                        else
                        {
                            geneLocationInfo = null;
                        }
                    }
                    else
                    {
                        geneLocationInfo?.AminoAcidSequence.AddRange(line.Select(aminoAcidCode => aminoAcidCode.ToAminoAcidName()));
                    }
                }
            }
            return peptides;
        }
    }
}