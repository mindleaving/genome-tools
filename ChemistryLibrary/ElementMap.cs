using System;

namespace ChemistryLibrary
{
    public static class ElementMap
    {
        public static ElementName FromProtonCount(int protons)
        {
            if(!Enum.IsDefined(typeof(ElementName), protons))
                throw new ArgumentOutOfRangeException($"Unknown element with {protons} protons");
            return (ElementName) protons;
        }

        public static ElementName FromSymbol(ElementSymbol symbol)
        {
            return (ElementName) symbol;
        }
    }
}