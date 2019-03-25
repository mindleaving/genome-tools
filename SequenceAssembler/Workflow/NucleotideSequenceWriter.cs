using System.Collections.Generic;
using System.IO;
using SequenceAssembler.Objects;

namespace SequenceAssembler.Workflow
{
    public class NucleotideSequenceWriter
    {
        public void Write(AssembledSequence assembledSequence, string outputFilePath)
        {
            var lines = new List<string>();
            var currentLine = string.Empty;
            foreach (var nucleotideByte in assembledSequence.Sequence)
            {
                var nucleotide = (Nucelotide) nucleotideByte;
                currentLine += nucleotide.ToString();
                if (currentLine.Length >= 80)
                {
                    lines.Add(currentLine);
                    currentLine = string.Empty;
                }
            }
            if(currentLine.Length > 0)
                lines.Add(currentLine);
            File.WriteAllLines(outputFilePath, lines);
        }
    }
}
