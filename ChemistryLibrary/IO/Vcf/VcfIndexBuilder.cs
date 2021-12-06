using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenomeTools.ChemistryLibrary.IO.Vcf
{
    /// <summary>
    /// The VCF Index implemented here is not part of the VCF specification.
    /// It contains an entry for every N-variant entries. N is specified on generation and is a tradeoff between index size and granularity.
    /// The following columns are included separated by TAB ('\t'):
    /// 0: Chromosome/sequence name
    /// 1: Variant position
    /// 2: File offset in bytes
    /// </summary>
    public class VcfIndexBuilder
    {
        public void Build(string filePath, int N = 10_000)
        {
            var indexEntries = Build(File.OpenRead(filePath), N);
            var outputFilePath = filePath + ".vcfi";
            File.WriteAllLines(outputFilePath, indexEntries.Select(x => $"{x.Chromosome}\t{x.Position}\t{x.FileOffset}"));
        }

        public List<VcfIndexEntry> Build(Stream vcfStream, int N = 10_000)
        {
            var indexEntries = new List<VcfIndexEntry>();
            using var streamReader = new UnbufferedStreamReader(vcfStream);

            var lineCount = 0;
            VcfHeader header = null;
            var lastChromosome = "";
            while (true)
            {
                var fileOffset = streamReader.Position;
                var line = streamReader.ReadLine();
                if(line == null)
                    break;
                if(line.StartsWith("##"))
                    continue;
                if (line.StartsWith("#"))
                {
                    header = VcfAccessor.ParseHeader(line);
                    continue;
                }
                var variantEntry = VcfAccessor.ParseVariant(line, header, null);
                if (variantEntry.Chromosome != lastChromosome) 
                    lineCount = 0;
                if (lineCount % N == 0)
                {
                    var indexEntry = new VcfIndexEntry(variantEntry.Chromosome, variantEntry.Position, fileOffset);
                    indexEntries.Add(indexEntry);
                }
                lineCount++;
                lastChromosome = variantEntry.Chromosome;
            }

            return indexEntries;
        }
    }
}