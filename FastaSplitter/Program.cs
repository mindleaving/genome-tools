using System;
using System.IO;
using System.Linq;

namespace FastaSplitter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var fileName = @"G:\Projects\HumanGenome\Homo_sapiens.GRCh38.dna.primary_assembly.fa";
            if (args.Length > 1)
                fileName = args[0];

            using (var streamReader = new StreamReader(fileName))
            {
                string line;
                StreamWriter streamWriter = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith(">"))
                    {
                        streamWriter?.Close();
                        var parsedHeader = line.Split(':',' ').ToList();
                        var referenceIndex = parsedHeader.IndexOf("GRCh38");
                        var description = parsedHeader[referenceIndex - 1] + "_" + parsedHeader[referenceIndex + 1];
                        var outputFileName = Path.Combine(Path.GetDirectoryName(fileName),
                            Path.GetFileNameWithoutExtension(fileName) 
                            + "." + description 
                            + Path.GetExtension(fileName));
                        streamWriter = new StreamWriter(outputFileName);
                    }
                    else
                    {
                        streamWriter.Write(line);
                    }
                }
                streamWriter?.Close();
            }
        }
    }
}
