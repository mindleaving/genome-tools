namespace GenomeTools.ChemistryLibrary.Genomics
{
    public enum ReferenceDnaStrandRole
    {
        Coding = 1,
        Plus = Coding,
        Sense = Coding,
        NonCoding = -1,
        Minus = NonCoding,
        Antisense = NonCoding,
    }
}
