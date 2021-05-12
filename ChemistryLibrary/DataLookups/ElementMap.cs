using System;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.DataLookups
{
    public static class ElementMap
    {
        public static ElementName FromProtonCount(int protons)
        {
            if(!Enum.IsDefined(typeof(ElementName), protons))
                throw new ArgumentOutOfRangeException($"Unknown element with {protons} protons");
            return (ElementName) protons;
        }

        public static ElementName ToElementName(this ElementSymbol symbol)
        {
            return (ElementName) symbol;
        }

        public static ElementSymbol ToElementSymbol(this ElementName name)
        {
            return (ElementSymbol)name;
        }
    }
}