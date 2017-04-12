namespace ChemistryLibrary.Pdb
{
    public class PdbHelixLine
    {
        public int SerialNumber { get; set; }
        public string Id { get; set; }
        public char FirstResidueChainId { get; set; }
        public string FirstResidueName { get; set; }
        public int FirstResidueNumber { get; set; }
        public char LastResidueChainId { get; set; }
        public string LastResidueName { get; set; }
        public int LastResidueNumber { get; set; }
        public HelixType Type { get; set; }
        public string Comment { get; set; }
        public int ResidueCount => LastResidueNumber - FirstResidueNumber + 1;
    }

    public enum HelixType
    {
        RightHandAlpha = 1,
        RightHandOmega = 2,
        RightHandPi = 3,
        RightHandGamma = 4,
        RightHand310 = 5,
        LeftHandAlpha = 6,
        LeftHandOmega = 7,
        LeftHandGamma = 8,
        Ribbon27 = 9,
        Polyproline = 10
    }
}
