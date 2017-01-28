using System;
using System.Collections.Generic;
using Commons;

namespace ChemistryLibrary
{
    public static class PeriodicTable
    {
        public static int GetProtonCount(ElementName element)
        {
            return (int) element;
        }

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

        private static readonly Dictionary<ElementName, double> ElectroNegativityLookup = new Dictionary<ElementName, double>
        {
            {ElementName.Hydrogen, 2.2},
            {ElementName.Helium, double.NaN},
            {ElementName.Lithium, 0.98},
            {ElementName.Beryllium, 1.57},
            {ElementName.Boron, 2.04},
            {ElementName.Carbon, 2.55},
            {ElementName.Nitrogen, 3.04},
            {ElementName.Oxygen, 3.44},
            {ElementName.Fluorine, 3.98},
            {ElementName.Neon, double.NaN},
            {ElementName.Sodium, 0.93},
            {ElementName.Magnesium, 1.31},
            {ElementName.Aluminium, 1.61},
            {ElementName.Silicon, 1.9},
            {ElementName.Phosphorus, 2.19},
            {ElementName.Sulfur, 2.58},
            {ElementName.Chlorine, 3.16},
            {ElementName.Argon, double.NaN},
            {ElementName.Potassium, 0.82},
            {ElementName.Calcium, 1},
            {ElementName.Scandium, 1.36},
            {ElementName.Titanium, 1.54},
            {ElementName.Vanadium, 1.63},
            {ElementName.Chromium, 1.66},
            {ElementName.Manganese, 1.55},
            {ElementName.Iron, 1.83},
            {ElementName.Cobalt, 1.88},
            {ElementName.Nickel, 1.91},
            {ElementName.Copper, 1.9},
            {ElementName.Zinc, 1.65},
            {ElementName.Gallium, 1.81},
            {ElementName.Germanium, 2.01},
            {ElementName.Arsenic, 2.18},
            {ElementName.Selenium, 2.55},
            {ElementName.Bromine, 2.96},
            {ElementName.Krypton, double.NaN},
            {ElementName.Rubidium, 0.82},
            {ElementName.Strontium, 0.95},
            {ElementName.Yttrium, 1.22},
            {ElementName.Zirconium, 1.33},
            {ElementName.Niobium, 1.6},
            {ElementName.Molybdenum, 2.16},
            {ElementName.Technetium, 1.9},
            {ElementName.Ruthenium, 2.2},
            {ElementName.Rhodium, 2.28},
            {ElementName.Palladium, 2.2},
            {ElementName.Silver, 1.93},
            {ElementName.Cadmium, 1.69},
            {ElementName.Indium, 1.78},
            {ElementName.Tin, 1.96},
            {ElementName.Antimony, 2.05},
            {ElementName.Tellurium, 2.1},
            {ElementName.Iodine, 2.66},
            {ElementName.Xenon, double.NaN},
            {ElementName.Cesium, 0.79},
            {ElementName.Barium, 0.89},
            {ElementName.Lanthanum, 1.1},
            {ElementName.Cerium, 1.12},
            {ElementName.Praseodymium, 1.13},
            {ElementName.Neodymium, 1.14},
            {ElementName.Promethium, 1.13},
            {ElementName.Samarium, 1.17},
            {ElementName.Europium, 1.2},
            {ElementName.Gadolinium, 1.2},
            {ElementName.Terbium, 1.2},
            {ElementName.Dysprosium, 1.22},
            {ElementName.Holmium, 1.23},
            {ElementName.Erbium, 1.24},
            {ElementName.Thulium, 1.25},
            {ElementName.Ytterbium, 1.1},
            {ElementName.Lutetium, 1.27},
            {ElementName.Hafnium, 1.3},
            {ElementName.Tantalum, 1.5},
            {ElementName.Tungsten, 2.36},
            {ElementName.Rhenium, 1.9},
            {ElementName.Osmium, 2.2},
            {ElementName.Iridium, 2.2},
            {ElementName.Platinum, 2.28},
            {ElementName.Gold, 2.54},
            {ElementName.Mercury, 2},
            {ElementName.Thallium, 2.04},
            {ElementName.Lead, 2.33},
            {ElementName.Bismuth, 2.02},
            {ElementName.Polonium, 2},
            {ElementName.Astatine, 2.2},
            {ElementName.Radon, double.NaN},
            {ElementName.Francium, 0.7},
            {ElementName.Radium, 0.9},
            {ElementName.Actinium, 1.1},
            {ElementName.Thorium, 1.3},
            {ElementName.Protactinium, 1.5},
            {ElementName.Uranium, 1.38},
            {ElementName.Neptunium, 1.36},
            {ElementName.Plutonium, 1.28},
            {ElementName.Americium, 1.3},
            {ElementName.Curium, 1.3},
            {ElementName.Berkelium, 1.3},
            {ElementName.Californium, 1.3},
            {ElementName.Einsteinium, 1.3},
            {ElementName.Fermium, 1.3},
            {ElementName.Mendelevium, 1.3},
            {ElementName.Nobelium, 1.3},
            {ElementName.Lawrencium, 1.3},
            {ElementName.Rutherfordium, double.NaN},
            {ElementName.Dubnium, double.NaN},
            {ElementName.Seaborgium, double.NaN},
            {ElementName.Bohrium, double.NaN},
            {ElementName.Hassium, double.NaN},
            {ElementName.Meitnerium, double.NaN},
            {ElementName.Darmstadtium, double.NaN},
            {ElementName.Roentgenium, double.NaN},
            {ElementName.Copernicium, double.NaN},
            {ElementName.Ununtrium, double.NaN},
            {ElementName.Ununquadium, double.NaN},
            {ElementName.Ununpentium, double.NaN},
            {ElementName.Ununhexium, double.NaN},
            {ElementName.Ununseptium, double.NaN},
            {ElementName.Ununoctium, double.NaN}
        };

        public static double ElectroNegativity(ElementName element)
        {
            return ElectroNegativityLookup[element];
        }

        private static readonly Dictionary<ElementName, UnitValue> AtomarWeight = new Dictionary<ElementName, UnitValue>
        {
            {ElementName.Hydrogen, 1.00794.To(Unit.GramPerMol)},
            {ElementName.Helium, 4.002602.To(Unit.GramPerMol)},
            {ElementName.Lithium, 6.941.To(Unit.GramPerMol)},
            {ElementName.Beryllium, 9.012182.To(Unit.GramPerMol)},
            {ElementName.Boron, 10.811.To(Unit.GramPerMol)},
            {ElementName.Carbon, 12.0107.To(Unit.GramPerMol)},
            {ElementName.Nitrogen, 14.0067.To(Unit.GramPerMol)},
            {ElementName.Oxygen, 15.9994.To(Unit.GramPerMol)},
            {ElementName.Fluorine, 18.9984032.To(Unit.GramPerMol)},
            {ElementName.Neon, 20.1797.To(Unit.GramPerMol)},
            {ElementName.Sodium, 22.98976928.To(Unit.GramPerMol)},
            {ElementName.Magnesium, 24.3050.To(Unit.GramPerMol)},
            {ElementName.Aluminium, 26.9815386.To(Unit.GramPerMol)},
            {ElementName.Silicon, 28.0855.To(Unit.GramPerMol)},
            {ElementName.Phosphorus, 30.973762.To(Unit.GramPerMol)},
            {ElementName.Sulfur, 32.065.To(Unit.GramPerMol)},
            {ElementName.Chlorine, 35.453.To(Unit.GramPerMol)},
            {ElementName.Argon, 39.948.To(Unit.GramPerMol)},
            {ElementName.Potassium, 39.0983.To(Unit.GramPerMol)},
            {ElementName.Calcium, 40.078.To(Unit.GramPerMol)},
            {ElementName.Scandium, 44.955912.To(Unit.GramPerMol)},
            {ElementName.Titanium, 47.867.To(Unit.GramPerMol)},
            {ElementName.Vanadium, 50.9415.To(Unit.GramPerMol)},
            {ElementName.Chromium, 51.9961.To(Unit.GramPerMol)},
            {ElementName.Manganese, 54.938045.To(Unit.GramPerMol)},
            {ElementName.Iron, 55.845.To(Unit.GramPerMol)},
            {ElementName.Cobalt, 58.933195.To(Unit.GramPerMol)},
            {ElementName.Nickel, 58.6934.To(Unit.GramPerMol)},
            {ElementName.Copper, 63.546.To(Unit.GramPerMol)},
            {ElementName.Zinc, 65.38.To(Unit.GramPerMol)},
            {ElementName.Gallium, 69.723.To(Unit.GramPerMol)},
            {ElementName.Germanium, 72.64.To(Unit.GramPerMol)},
            {ElementName.Arsenic, 74.92160.To(Unit.GramPerMol)},
            {ElementName.Selenium, 78.96.To(Unit.GramPerMol)},
            {ElementName.Bromine, 79.904.To(Unit.GramPerMol)},
            {ElementName.Krypton, 83.798.To(Unit.GramPerMol)},
            {ElementName.Rubidium, 85.4678.To(Unit.GramPerMol)},
            {ElementName.Strontium, 87.62.To(Unit.GramPerMol)},
            {ElementName.Yttrium, 88.90585.To(Unit.GramPerMol)},
            {ElementName.Zirconium, 91.224.To(Unit.GramPerMol)},
            {ElementName.Niobium, 92.90638.To(Unit.GramPerMol)},
            {ElementName.Molybdenum, 95.96.To(Unit.GramPerMol)},
            {ElementName.Technetium, 98.To(Unit.GramPerMol)},
            {ElementName.Ruthenium, 101.07.To(Unit.GramPerMol)},
            {ElementName.Rhodium, 102.90550.To(Unit.GramPerMol)},
            {ElementName.Palladium, 106.42.To(Unit.GramPerMol)},
            {ElementName.Silver, 107.8682.To(Unit.GramPerMol)},
            {ElementName.Cadmium, 112.411.To(Unit.GramPerMol)},
            {ElementName.Indium, 114.818.To(Unit.GramPerMol)},
            {ElementName.Tin, 118.710.To(Unit.GramPerMol)},
            {ElementName.Antimony, 121.760.To(Unit.GramPerMol)},
            {ElementName.Tellurium, 127.60.To(Unit.GramPerMol)},
            {ElementName.Iodine, 126.90447.To(Unit.GramPerMol)},
            {ElementName.Xenon, 131.293.To(Unit.GramPerMol)},
            {ElementName.Cesium, 132.9054519.To(Unit.GramPerMol)},
            {ElementName.Barium, 137.327.To(Unit.GramPerMol)},
            {ElementName.Lanthanum, 138.90547.To(Unit.GramPerMol)},
            {ElementName.Cerium, 140.116.To(Unit.GramPerMol)},
            {ElementName.Praseodymium, 140.90765.To(Unit.GramPerMol)},
            {ElementName.Neodymium, 144.242.To(Unit.GramPerMol)},
            {ElementName.Promethium, 145.To(Unit.GramPerMol)},
            {ElementName.Samarium, 150.36.To(Unit.GramPerMol)},
            {ElementName.Europium, 151.964.To(Unit.GramPerMol)},
            {ElementName.Gadolinium, 157.25.To(Unit.GramPerMol)},
            {ElementName.Terbium, 158.92535.To(Unit.GramPerMol)},
            {ElementName.Dysprosium, 162.500.To(Unit.GramPerMol)},
            {ElementName.Holmium, 164.93032.To(Unit.GramPerMol)},
            {ElementName.Erbium, 167.259.To(Unit.GramPerMol)},
            {ElementName.Thulium, 168.93421.To(Unit.GramPerMol)},
            {ElementName.Ytterbium, 173.054.To(Unit.GramPerMol)},
            {ElementName.Lutetium, 174.9668.To(Unit.GramPerMol)},
            {ElementName.Hafnium, 178.49.To(Unit.GramPerMol)},
            {ElementName.Tantalum, 180.94788.To(Unit.GramPerMol)},
            {ElementName.Tungsten, 183.84.To(Unit.GramPerMol)},
            {ElementName.Rhenium, 186.207.To(Unit.GramPerMol)},
            {ElementName.Osmium, 190.23.To(Unit.GramPerMol)},
            {ElementName.Iridium, 192.217.To(Unit.GramPerMol)},
            {ElementName.Platinum, 195.084.To(Unit.GramPerMol)},
            {ElementName.Gold, 196.966569.To(Unit.GramPerMol)},
            {ElementName.Mercury, 200.59.To(Unit.GramPerMol)},
            {ElementName.Thallium, 204.3833.To(Unit.GramPerMol)},
            {ElementName.Lead, 207.2.To(Unit.GramPerMol)},
            {ElementName.Bismuth, 208.98040.To(Unit.GramPerMol)},
            {ElementName.Polonium, 209.To(Unit.GramPerMol)},
            {ElementName.Astatine, 210.To(Unit.GramPerMol)},
            {ElementName.Radon, 222.To(Unit.GramPerMol)},
            {ElementName.Francium, 223.To(Unit.GramPerMol)},
            {ElementName.Radium, 226.To(Unit.GramPerMol)},
            {ElementName.Actinium, 227.To(Unit.GramPerMol)},
            {ElementName.Thorium, 232.03806.To(Unit.GramPerMol)},
            {ElementName.Protactinium, 231.03588.To(Unit.GramPerMol)},
            {ElementName.Uranium, 238.02891.To(Unit.GramPerMol)},
            {ElementName.Neptunium, 237.To(Unit.GramPerMol)},
            {ElementName.Plutonium, 244.To(Unit.GramPerMol)},
            {ElementName.Americium, 243.To(Unit.GramPerMol)},
            {ElementName.Curium, 247.To(Unit.GramPerMol)},
            {ElementName.Berkelium, 247.To(Unit.GramPerMol)},
            {ElementName.Californium, 251.To(Unit.GramPerMol)},
            {ElementName.Einsteinium, 252.To(Unit.GramPerMol)},
            {ElementName.Fermium, 257.To(Unit.GramPerMol)},
            {ElementName.Mendelevium, 258.To(Unit.GramPerMol)},
            {ElementName.Nobelium, 259.To(Unit.GramPerMol)},
            {ElementName.Lawrencium, 262.To(Unit.GramPerMol)},
            {ElementName.Rutherfordium, 267.To(Unit.GramPerMol)},
            {ElementName.Dubnium, 268.To(Unit.GramPerMol)},
            {ElementName.Seaborgium, 271.To(Unit.GramPerMol)},
            {ElementName.Bohrium, 272.To(Unit.GramPerMol)},
            {ElementName.Hassium, 270.To(Unit.GramPerMol)},
            {ElementName.Meitnerium, 276.To(Unit.GramPerMol)},
            {ElementName.Darmstadtium, 281.To(Unit.GramPerMol)},
            {ElementName.Roentgenium, 280.To(Unit.GramPerMol)},
            {ElementName.Copernicium, 285.To(Unit.GramPerMol)},
            {ElementName.Ununtrium, 284.To(Unit.GramPerMol)},
            {ElementName.Ununquadium, 289.To(Unit.GramPerMol)},
            {ElementName.Ununpentium, 288.To(Unit.GramPerMol)},
            {ElementName.Ununhexium, 293.To(Unit.GramPerMol)},
            {ElementName.Ununseptium, double.NaN.To(Unit.GramPerMol)},
            {ElementName.Ununoctium, 294.To(Unit.GramPerMol)}
        };

        public static UnitValue GetMass(ElementName element)
        {
            return AtomarWeight[element];
        }

        private static readonly Dictionary<ElementName, UnitValue> AtomRadius = new Dictionary<ElementName, UnitValue>
        {
            {ElementName.Hydrogen, 37.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Helium, 32.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lithium, 134.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Beryllium, 90.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Boron, 82.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Carbon, 77.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Nitrogen, 75.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Oxygen, 73.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Fluorine, 71.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Neon, 69.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Sodium, 154.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Magnesium, 130.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Aluminium, 118.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Silicon, 111.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Phosphorus, 106.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Sulfur, 102.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Chlorine, 99.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Argon, 97.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Potassium, 196.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Calcium, 174.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Scandium, 144.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Titanium, 136.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Vanadium, 125.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Chromium, 127.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Manganese, 139.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Iron, 125.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Cobalt, 126.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Nickel, 121.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Copper, 138.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Zinc, 131.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Gallium, 126.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Germanium, 122.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Arsenic, 119.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Selenium, 116.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Bromine, 114.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Krypton, 110.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Rubidium, 211.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Strontium, 192.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Yttrium, 162.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Zirconium, 148.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Niobium, 137.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Molybdenum, 145.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Technetium, 156.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ruthenium, 126.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Rhodium, 135.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Palladium, 131.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Silver, 153.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Cadmium, 148.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Indium, 144.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tin, 141.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Antimony, 138.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tellurium, 135.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Iodine, 133.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Xenon, 130.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Cesium, 225.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Barium, 198.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lanthanum, 169.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Cerium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Praseodymium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Neodymium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Promethium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Samarium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Europium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Gadolinium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Terbium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Dysprosium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Holmium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Erbium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Thulium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ytterbium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lutetium, 160.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Hafnium, 150.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tantalum, 138.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tungsten, 146.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Rhenium, 159.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Osmium, 128.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Iridium, 137.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Platinum, 128.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Gold, 144.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Mercury, 149.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Thallium, 148.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lead, 147.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Bismuth, 146.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Polonium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Astatine, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Radon, 145.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Francium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Radium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Actinium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Thorium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Protactinium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Uranium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Neptunium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Plutonium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Americium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Curium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Berkelium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Californium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Einsteinium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Fermium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Mendelevium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Nobelium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lawrencium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Rutherfordium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Dubnium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Seaborgium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Bohrium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Hassium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Meitnerium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Darmstadtium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Roentgenium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Copernicium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ununtrium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ununquadium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ununpentium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ununhexium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ununseptium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ununoctium, double.NaN.To(SIPrefix.Pico, Unit.Meter)}
        };

        public static UnitValue GetRadius(ElementName element)
        {
            return AtomRadius[element];
        }
    }
}