using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Tools
{
    [TestFixture]
    public class FastaSplitter
    {
        [Test]
        [TestCase(@"F:\HumanGenome\Homo_sapiens.GRCh38.dna.primary_assembly.fa")]
        public void Split(string fastaFilename)
        {
            using (var streamReader = new StreamReader(fastaFilename))
            {
                string line;
                StreamWriter streamWriter = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith(">"))
                    {
                        streamWriter?.Close();
                        var parsedHeader = line.Split(':', ' ').ToList();
                        var referenceIndex = parsedHeader.IndexOf("GRCh38");
                        var description = parsedHeader[referenceIndex - 1] + "_" + parsedHeader[referenceIndex + 1];
                        var outputFileName = Path.Combine(Path.GetDirectoryName(fastaFilename),
                            Path.GetFileNameWithoutExtension(fastaFilename)
                            + "." + description
                            + Path.GetExtension(fastaFilename));
                        streamWriter = new StreamWriter(outputFileName);
                    }
                    else
                    {
                        streamWriter?.Write(line);
                    }
                }
                streamWriter?.Close();
            }
        }
    }
}
