using System;

namespace ChemistryLibrary
{
    public static class PeriodicTable
    {
        public static int GetPeriod(int elementNr)
        {
            return GetPeriod(ElementMap.FromProtonCount(elementNr));
        }
        public static int GetPeriod(ElementName element)
        {
            switch (element)
            {
                case ElementName.Hydrogen:
                case ElementName.Helium:
                    return 1;
                case ElementName.Lithium:
                case ElementName.Beryllium:
                case ElementName.Boron:
                case ElementName.Carbon:
                case ElementName.Nitrogen:
                case ElementName.Oxygen:
                case ElementName.Fluorine:
                case ElementName.Neon:
                    return 2;
                case ElementName.Sodium:
                case ElementName.Magnesium:
                case ElementName.Aluminium:
                case ElementName.Silicon:
                case ElementName.Phosphorus:
                case ElementName.Sulfur:
                case ElementName.Chlorine:
                case ElementName.Argon:
                    return 3;
                case ElementName.Potassium:
                case ElementName.Calcium:
                case ElementName.Scandium:
                case ElementName.Titanium:
                case ElementName.Vanadium:
                case ElementName.Chromium:
                case ElementName.Manganese:
                case ElementName.Iron:
                case ElementName.Cobalt:
                case ElementName.Nickel:
                case ElementName.Copper:
                case ElementName.Zinc:
                case ElementName.Gallium:
                case ElementName.Germanium:
                case ElementName.Arsenic:
                case ElementName.Selenium:
                case ElementName.Bromine:
                case ElementName.Krypton:
                    return 4;
                case ElementName.Rubidium:
                case ElementName.Strontium:
                case ElementName.Yttrium:
                case ElementName.Zirconium:
                case ElementName.Niobium:
                case ElementName.Molybdenum:
                case ElementName.Technetium:
                case ElementName.Ruthenium:
                case ElementName.Rhodium:
                case ElementName.Palladium:
                case ElementName.Silver:
                case ElementName.Cadmium:
                case ElementName.Indium:
                case ElementName.Tin:
                case ElementName.Antimony:
                case ElementName.Tellurium:
                case ElementName.Iodine:
                case ElementName.Xenon:
                    return 5;
                case ElementName.Cesium:
                case ElementName.Barium:
                case ElementName.Lanthanum:
                case ElementName.Cerium:
                case ElementName.Praseodymium:
                case ElementName.Neodymium:
                case ElementName.Promethium:
                case ElementName.Samarium:
                case ElementName.Europium:
                case ElementName.Gadolinium:
                case ElementName.Terbium:
                case ElementName.Dysprosium:
                case ElementName.Holmium:
                case ElementName.Erbium:
                case ElementName.Thulium:
                case ElementName.Ytterbium:
                case ElementName.Lutetium:
                case ElementName.Hafnium:
                case ElementName.Tantalum:
                case ElementName.Tungsten:
                case ElementName.Rhenium:
                case ElementName.Osmium:
                case ElementName.Iridium:
                case ElementName.Platinum:
                case ElementName.Gold:
                case ElementName.Mercury:
                case ElementName.Thallium:
                case ElementName.Lead:
                case ElementName.Bismuth:
                case ElementName.Polonium:
                case ElementName.Astatine:
                case ElementName.Radon:
                    return 6;
                case ElementName.Francium:
                case ElementName.Radium:
                case ElementName.Actinium:
                case ElementName.Thorium:
                case ElementName.Protactinium:
                case ElementName.Uranium:
                case ElementName.Neptunium:
                case ElementName.Plutonium:
                case ElementName.Americium:
                case ElementName.Curium:
                case ElementName.Berkelium:
                case ElementName.Californium:
                case ElementName.Einsteinium:
                case ElementName.Fermium:
                case ElementName.Mendelevium:
                case ElementName.Nobelium:
                case ElementName.Lawrencium:
                case ElementName.Rutherfordium:
                case ElementName.Dubnium:
                case ElementName.Seaborgium:
                case ElementName.Bohrium:
                case ElementName.Hassium:
                case ElementName.Meitnerium:
                case ElementName.Darmstadtium:
                case ElementName.Roentgenium:
                case ElementName.Copernicium:
                case ElementName.Ununtrium:
                case ElementName.Ununquadium:
                case ElementName.Ununpentium:
                case ElementName.Ununhexium:
                case ElementName.Ununseptium:
                case ElementName.Ununoctium:
                    return 7;
                default:
                    throw new ArgumentOutOfRangeException(nameof(element), element, null);
            }
        }

    }
}