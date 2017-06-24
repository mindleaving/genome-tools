using ChemistryLibrary.Objects;

namespace ChemistryLibrary.IO.Pdb
{
    public class PdbAtomLine
    {
        public int SerialNumber { get; set; }
        public string Name { get; set; }
        public string ResidueName { get; set; }
        public char ChainId { get; set; }
        public char AlternateConformationId { get; set; }
        public int ResidueNumber { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Occupancy { get; set; }
        public double TemperatureFactor { get; set; }
        public ElementSymbol Element { get; set; }
        public int Charge { get; set; }
    }
}
