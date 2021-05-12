namespace GenomeTools.ChemistryLibrary.IO.Pdb
{
    public class PdbSheetLine
    {
        public int StrandSerialNumber { get; set; }
        public string SheetId { get; set; }
        public int StrandCount { get; set; }
        public char FirstResidueChainId { get; set; }
        public string FirstResidueName { get; set; }
        public int FirstResidueNumber { get; set; }
        public char LastResidueChainId { get; set; }
        public string LastResidueName { get; set; }
        public int LastResidueNumber { get; set; }
        public SheetStrandSense StrandSense { get; set; }
        public int ResidueCount => LastResidueNumber - FirstResidueNumber + 1;
    }

    public enum SheetStrandSense
    {
        FirstStrand = 0,
        Parallel = 1,
        AntiParallel = -1
    }
}
