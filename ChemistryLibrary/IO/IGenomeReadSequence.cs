namespace GenomeTools.ChemistryLibrary.IO
{
    public interface IGenomeReadSequence
    {
        int Length { get; }

        /// <summary>
        /// Get single base from this read
        /// </summary>
        /// <param name="readIndex">
        /// Zero-based index of base relative to read,
        /// i.e. 0 is the first base of the read in the 5'->3' direction
        /// </param>
        char GetBase(int readIndex);

        /// <summary>
        /// Read stretch of bases from this read
        /// </summary>
        /// <param name="readStartIndex">
        /// Zero-based index of the first base to get relative to the read,
        /// i.e. 0 is the first base of the read in the 5'->3' direction
        /// </param>
        /// <param name="readEndIndex">
        /// Zero-based index of last base to get relative to the read,
        /// i.e. 0 is the first base of the read in the 5'->3' direction
        /// </param>
        string GetBases(int readStartIndex, int readEndIndex);

        /// <summary>
        /// Get whole sequence
        /// </summary>
        string GetSequence();
    }
}