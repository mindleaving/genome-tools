namespace GenomeTools.ChemistryLibrary.IO.Fasta
{
    public class FastaIndexEntry
    {
        public string SequenceName { get; }
        public long Length { get; set; }
        public long FirstBaseOffset { get; }
        public ushort BasesPerLine { get; }
        public ushort LineWidth { get; }

        public FastaIndexEntry(
            string sequenceName,
            long firstBaseOffset, 
            ushort basesPerLine,
            ushort lineWidth)
        {
            SequenceName = sequenceName;
            FirstBaseOffset = firstBaseOffset;
            BasesPerLine = basesPerLine;
            LineWidth = lineWidth;
        }
    }
}