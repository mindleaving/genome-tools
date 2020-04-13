using System;
using System.Collections.Generic;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Physics;

namespace ChemistryLibrary.DataLookups
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
            {ElementName.Hydrogen, 1.00794e-3.To(Unit.KilogramPerMole)},
            {ElementName.Helium, 4.002602e-3.To(Unit.KilogramPerMole)},
            {ElementName.Lithium, 6.941e-3.To(Unit.KilogramPerMole)},
            {ElementName.Beryllium, 9.012182e-3.To(Unit.KilogramPerMole)},
            {ElementName.Boron, 10.811e-3.To(Unit.KilogramPerMole)},
            {ElementName.Carbon, 12.0107e-3.To(Unit.KilogramPerMole)},
            {ElementName.Nitrogen, 14.0067e-3.To(Unit.KilogramPerMole)},
            {ElementName.Oxygen, 15.9994e-3.To(Unit.KilogramPerMole)},
            {ElementName.Fluorine, 18.9984032e-3.To(Unit.KilogramPerMole)},
            {ElementName.Neon, 20.1797e-3.To(Unit.KilogramPerMole)},
            {ElementName.Sodium, 22.98976928e-3.To(Unit.KilogramPerMole)},
            {ElementName.Magnesium, 24.3050e-3.To(Unit.KilogramPerMole)},
            {ElementName.Aluminium, 26.9815386e-3.To(Unit.KilogramPerMole)},
            {ElementName.Silicon, 28.0855e-3.To(Unit.KilogramPerMole)},
            {ElementName.Phosphorus, 30.973762e-3.To(Unit.KilogramPerMole)},
            {ElementName.Sulfur, 32.065e-3.To(Unit.KilogramPerMole)},
            {ElementName.Chlorine, 35.453e-3.To(Unit.KilogramPerMole)},
            {ElementName.Argon, 39.948e-3.To(Unit.KilogramPerMole)},
            {ElementName.Potassium, 39.0983e-3.To(Unit.KilogramPerMole)},
            {ElementName.Calcium, 40.078e-3.To(Unit.KilogramPerMole)},
            {ElementName.Scandium, 44.955912e-3.To(Unit.KilogramPerMole)},
            {ElementName.Titanium, 47.867e-3.To(Unit.KilogramPerMole)},
            {ElementName.Vanadium, 50.9415e-3.To(Unit.KilogramPerMole)},
            {ElementName.Chromium, 51.9961e-3.To(Unit.KilogramPerMole)},
            {ElementName.Manganese, 54.938045e-3.To(Unit.KilogramPerMole)},
            {ElementName.Iron, 55.845e-3.To(Unit.KilogramPerMole)},
            {ElementName.Cobalt, 58.933195e-3.To(Unit.KilogramPerMole)},
            {ElementName.Nickel, 58.6934e-3.To(Unit.KilogramPerMole)},
            {ElementName.Copper, 63.546e-3.To(Unit.KilogramPerMole)},
            {ElementName.Zinc, 65.38e-3.To(Unit.KilogramPerMole)},
            {ElementName.Gallium, 69.723e-3.To(Unit.KilogramPerMole)},
            {ElementName.Germanium, 72.64e-3.To(Unit.KilogramPerMole)},
            {ElementName.Arsenic, 74.92160e-3.To(Unit.KilogramPerMole)},
            {ElementName.Selenium, 78.96e-3.To(Unit.KilogramPerMole)},
            {ElementName.Bromine, 79.904e-3.To(Unit.KilogramPerMole)},
            {ElementName.Krypton, 83.798e-3.To(Unit.KilogramPerMole)},
            {ElementName.Rubidium, 85.4678e-3.To(Unit.KilogramPerMole)},
            {ElementName.Strontium, 87.62e-3.To(Unit.KilogramPerMole)},
            {ElementName.Yttrium, 88.90585e-3.To(Unit.KilogramPerMole)},
            {ElementName.Zirconium, 91.224e-3.To(Unit.KilogramPerMole)},
            {ElementName.Niobium, 92.90638e-3.To(Unit.KilogramPerMole)},
            {ElementName.Molybdenum, 95.96e-3.To(Unit.KilogramPerMole)},
            {ElementName.Technetium, 98e-3.To(Unit.KilogramPerMole)},
            {ElementName.Ruthenium, 101.07e-3.To(Unit.KilogramPerMole)},
            {ElementName.Rhodium, 102.90550e-3.To(Unit.KilogramPerMole)},
            {ElementName.Palladium, 106.42e-3.To(Unit.KilogramPerMole)},
            {ElementName.Silver, 107.8682e-3.To(Unit.KilogramPerMole)},
            {ElementName.Cadmium, 112.411e-3.To(Unit.KilogramPerMole)},
            {ElementName.Indium, 114.818e-3.To(Unit.KilogramPerMole)},
            {ElementName.Tin, 118.710e-3.To(Unit.KilogramPerMole)},
            {ElementName.Antimony, 121.760e-3.To(Unit.KilogramPerMole)},
            {ElementName.Tellurium, 127.60e-3.To(Unit.KilogramPerMole)},
            {ElementName.Iodine, 126.90447e-3.To(Unit.KilogramPerMole)},
            {ElementName.Xenon, 131.293e-3.To(Unit.KilogramPerMole)},
            {ElementName.Cesium, 132.9054519e-3.To(Unit.KilogramPerMole)},
            {ElementName.Barium, 137.327e-3.To(Unit.KilogramPerMole)},
            {ElementName.Lanthanum, 138.90547e-3.To(Unit.KilogramPerMole)},
            {ElementName.Cerium, 140.116e-3.To(Unit.KilogramPerMole)},
            {ElementName.Praseodymium, 140.90765e-3.To(Unit.KilogramPerMole)},
            {ElementName.Neodymium, 144.242e-3.To(Unit.KilogramPerMole)},
            {ElementName.Promethium, 145e-3.To(Unit.KilogramPerMole)},
            {ElementName.Samarium, 150.36e-3.To(Unit.KilogramPerMole)},
            {ElementName.Europium, 151.964e-3.To(Unit.KilogramPerMole)},
            {ElementName.Gadolinium, 157.25e-3.To(Unit.KilogramPerMole)},
            {ElementName.Terbium, 158.92535e-3.To(Unit.KilogramPerMole)},
            {ElementName.Dysprosium, 162.500e-3.To(Unit.KilogramPerMole)},
            {ElementName.Holmium, 164.93032e-3.To(Unit.KilogramPerMole)},
            {ElementName.Erbium, 167.259e-3.To(Unit.KilogramPerMole)},
            {ElementName.Thulium, 168.93421e-3.To(Unit.KilogramPerMole)},
            {ElementName.Ytterbium, 173.054e-3.To(Unit.KilogramPerMole)},
            {ElementName.Lutetium, 174.9668e-3.To(Unit.KilogramPerMole)},
            {ElementName.Hafnium, 178.49e-3.To(Unit.KilogramPerMole)},
            {ElementName.Tantalum, 180.94788e-3.To(Unit.KilogramPerMole)},
            {ElementName.Tungsten, 183.84e-3.To(Unit.KilogramPerMole)},
            {ElementName.Rhenium, 186.207e-3.To(Unit.KilogramPerMole)},
            {ElementName.Osmium, 190.23e-3.To(Unit.KilogramPerMole)},
            {ElementName.Iridium, 192.217e-3.To(Unit.KilogramPerMole)},
            {ElementName.Platinum, 195.084e-3.To(Unit.KilogramPerMole)},
            {ElementName.Gold, 196.966569e-3.To(Unit.KilogramPerMole)},
            {ElementName.Mercury, 200.59e-3.To(Unit.KilogramPerMole)},
            {ElementName.Thallium, 204.3833e-3.To(Unit.KilogramPerMole)},
            {ElementName.Lead, 207.2e-3.To(Unit.KilogramPerMole)},
            {ElementName.Bismuth, 208.98040e-3.To(Unit.KilogramPerMole)},
            {ElementName.Polonium, 209e-3.To(Unit.KilogramPerMole)},
            {ElementName.Astatine, 210e-3.To(Unit.KilogramPerMole)},
            {ElementName.Radon, 222e-3.To(Unit.KilogramPerMole)},
            {ElementName.Francium, 223e-3.To(Unit.KilogramPerMole)},
            {ElementName.Radium, 226e-3.To(Unit.KilogramPerMole)},
            {ElementName.Actinium, 227e-3.To(Unit.KilogramPerMole)},
            {ElementName.Thorium, 232.03806e-3.To(Unit.KilogramPerMole)},
            {ElementName.Protactinium, 231.03588e-3.To(Unit.KilogramPerMole)},
            {ElementName.Uranium, 238.02891e-3.To(Unit.KilogramPerMole)},
            {ElementName.Neptunium, 237e-3.To(Unit.KilogramPerMole)},
            {ElementName.Plutonium, 244e-3.To(Unit.KilogramPerMole)},
            {ElementName.Americium, 243e-3.To(Unit.KilogramPerMole)},
            {ElementName.Curium, 247e-3.To(Unit.KilogramPerMole)},
            {ElementName.Berkelium, 247e-3.To(Unit.KilogramPerMole)},
            {ElementName.Californium, 251e-3.To(Unit.KilogramPerMole)},
            {ElementName.Einsteinium, 252e-3.To(Unit.KilogramPerMole)},
            {ElementName.Fermium, 257e-3.To(Unit.KilogramPerMole)},
            {ElementName.Mendelevium, 258e-3.To(Unit.KilogramPerMole)},
            {ElementName.Nobelium, 259e-3.To(Unit.KilogramPerMole)},
            {ElementName.Lawrencium, 262e-3.To(Unit.KilogramPerMole)},
            {ElementName.Rutherfordium, 267e-3.To(Unit.KilogramPerMole)},
            {ElementName.Dubnium, 268e-3.To(Unit.KilogramPerMole)},
            {ElementName.Seaborgium, 271e-3.To(Unit.KilogramPerMole)},
            {ElementName.Bohrium, 272e-3.To(Unit.KilogramPerMole)},
            {ElementName.Hassium, 270e-3.To(Unit.KilogramPerMole)},
            {ElementName.Meitnerium, 276e-3.To(Unit.KilogramPerMole)},
            {ElementName.Darmstadtium, 281e-3.To(Unit.KilogramPerMole)},
            {ElementName.Roentgenium, 280e-3.To(Unit.KilogramPerMole)},
            {ElementName.Copernicium, 285e-3.To(Unit.KilogramPerMole)},
            {ElementName.Ununtrium, 284e-3.To(Unit.KilogramPerMole)},
            {ElementName.Ununquadium, 289e-3.To(Unit.KilogramPerMole)},
            {ElementName.Ununpentium, 288e-3.To(Unit.KilogramPerMole)},
            {ElementName.Ununhexium, 293e-3.To(Unit.KilogramPerMole)},
            {ElementName.Ununseptium, double.NaN.To(Unit.KilogramPerMole)},
            {ElementName.Ununoctium, 294e-3.To(Unit.KilogramPerMole)}
        };

        public static UnitValue GetMass(ElementName element)
        {
            return AtomarWeight[element];
        }

        public static UnitValue GetSingleAtomMass(ElementName element)
        {
            return GetMass(element) / PhysicalConstants.AvogradrosNumber;
        }

        private static readonly Dictionary<ElementName, UnitValue> CovalentAtomRadius = new Dictionary<ElementName, UnitValue>
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

        public static UnitValue GetCovalentRadius(ElementName element)
        {
            return CovalentAtomRadius[element];
        }

        // Source: https://en.wikipedia.org/wiki/Van_der_Waals_radius
        private static readonly Dictionary<ElementName, UnitValue> VanDerWaalsAtomRadius = new Dictionary<ElementName, UnitValue>
        {
            {ElementName.Hydrogen, 110.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Helium, 140.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lithium, 182.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Beryllium, 153.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Boron, 192.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Carbon, 170.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Nitrogen, 155.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Oxygen, 152.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Fluorine, 147.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Neon, 154.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Sodium, 227.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Magnesium, 173.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Aluminium, 184.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Silicon, 210.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Phosphorus, 180.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Sulfur, 180.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Chlorine, 175.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Argon, 188.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Potassium, 275.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Calcium, 231.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Scandium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Titanium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Vanadium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Chromium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Manganese, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Iron, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Cobalt, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Nickel, 163.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Copper, 140.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Zinc, 139.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Gallium, 187.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Germanium, 211.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Arsenic, 185.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Selenium, 190.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Bromine, 185.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Krypton, 202.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Rubidium, 303.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Strontium, 268.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Yttrium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Zirconium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Niobium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Molybdenum, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Technetium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Ruthenium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Rhodium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Palladium, 163.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Silver, 172.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Cadmium, 158.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Indium, 193.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tin, 217.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Antimony, 206.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tellurium, 206.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Iodine, 198.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Xenon, 216.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Cesium, 343.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Barium, 268.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lanthanum, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
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
            {ElementName.Lutetium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Hafnium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tantalum, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Tungsten, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Rhenium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Osmium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Iridium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Platinum, 175.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Gold, 166.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Mercury, 155.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Thallium, 196.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Lead, 202.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Bismuth, 207.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Polonium, 197.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Astatine, 202.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Radon, 220.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Francium, 348.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Radium, 283.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Actinium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Thorium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Protactinium, double.NaN.To(SIPrefix.Pico, Unit.Meter)},
            {ElementName.Uranium, 186.To(SIPrefix.Pico, Unit.Meter)},
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

        public static UnitValue GetVanDerWaalsRadius(ElementName element)
        {
            return CovalentAtomRadius[element];
        }
    }
}