﻿using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.DataLookups
{
    public static class IsotopeTable
    {
        public static readonly List<IsotopeInfo> StableIsotopes = new List<IsotopeInfo>
        {
            new IsotopeInfo(ElementName.Hydrogen, 1, 0, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Hydrogen, 1, 1, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Helium, 2, 1, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Helium, 2, 2, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lithium, 3, 3, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lithium, 3, 4, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Beryllium, 4, 5, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Boron, 5, 5, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Boron, 5, 6, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Carbon, 6, 6, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Carbon, 6, 7, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Nitrogen, 7, 7, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Nitrogen, 7, 8, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Oxygen, 8, 8, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Oxygen, 8, 9, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Oxygen, 8, 10, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Fluorine, 9, 10, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neon, 10, 10, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neon, 10, 11, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neon, 10, 12, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Sodium, 11, 12, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Magnesium, 12, 12, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Magnesium, 12, 13, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Magnesium, 12, 14, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Aluminium, 13, 14, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Silicon, 14, 14, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Silicon, 14, 15, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Silicon, 14, 16, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Phosphorus, 15, 16, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Sulfur, 16, 16, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Sulfur, 16, 17, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Sulfur, 16, 18, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Sulfur, 16, 20, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Chlorine, 17, 18, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Chlorine, 17, 20, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Argon, 18, 18, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Argon, 18, 20, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Argon, 18, 22, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Potassium, 19, 20, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Potassium, 19, 22, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Calcium, 20, 20, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Calcium, 20, 22, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Calcium, 20, 23, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Calcium, 20, 24, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Calcium, 20, 26, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Scandium, 21, 24, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Titanium, 22, 24, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Titanium, 22, 25, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Titanium, 22, 26, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Titanium, 22, 27, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Titanium, 22, 28, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Vanadium, 23, 28, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Chromium, 24, 26, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Chromium, 24, 28, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Chromium, 24, 29, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Chromium, 24, 30, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Manganese, 25, 30, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Iron, 26, 28, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Iron, 26, 30, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Iron, 26, 31, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Iron, 26, 32, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cobalt, 27, 32, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Nickel, 28, 30, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Nickel, 28, 32, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Nickel, 28, 33, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Nickel, 28, 34, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Nickel, 28, 36, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Copper, 29, 34, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Copper, 29, 36, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zinc, 30, 34, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zinc, 30, 36, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zinc, 30, 37, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zinc, 30, 38, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zinc, 30, 40, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gallium, 31, 38, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gallium, 31, 40, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Germanium, 32, 38, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Germanium, 32, 40, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Germanium, 32, 41, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Germanium, 32, 42, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Arsenic, 33, 42, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Selenium, 34, 40, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Selenium, 34, 42, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Selenium, 34, 43, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Selenium, 34, 44, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Selenium, 34, 46, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Bromine, 35, 44, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Bromine, 35, 46, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Krypton, 36, 42, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Krypton, 36, 44, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Krypton, 36, 46, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Krypton, 36, 47, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Krypton, 36, 48, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Krypton, 36, 50, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Rubidium, 37, 48, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Strontium, 38, 46, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Strontium, 38, 48, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Strontium, 38, 49, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Strontium, 38, 50, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Yttrium, 39, 50, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zirconium, 40, 50, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zirconium, 40, 51, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zirconium, 40, 52, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Zirconium, 40, 54, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Niobium, 41, 52, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Molybdenum, 42, 50, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Molybdenum, 42, 52, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Molybdenum, 42, 53, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Molybdenum, 42, 54, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Molybdenum, 42, 55, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Molybdenum, 42, 56, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ruthenium, 44, 52, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ruthenium, 44, 54, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ruthenium, 44, 55, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ruthenium, 44, 56, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ruthenium, 44, 57, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ruthenium, 44, 58, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ruthenium, 44, 60, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Rhodium, 45, 58, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Palladium, 46, 56, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Palladium, 46, 58, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Palladium, 46, 59, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Palladium, 46, 60, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Palladium, 46, 62, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Palladium, 46, 64, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Silver, 47, 60, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Silver, 47, 62, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cadmium, 48, 58, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cadmium, 48, 60, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cadmium, 48, 62, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cadmium, 48, 63, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cadmium, 48, 64, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cadmium, 48, 66, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Indium, 49, 64, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 62, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 64, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 65, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 66, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 67, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 68, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 69, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 70, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 72, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tin, 50, 74, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Antimony, 51, 70, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Antimony, 51, 72, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tellurium, 52, 68, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tellurium, 52, 70, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tellurium, 52, 71, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tellurium, 52, 72, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tellurium, 52, 73, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tellurium, 52, 74, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Iodine, 53, 74, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 70, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 72, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 74, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 75, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 76, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 77, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 78, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Xenon, 54, 80, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cesium, 55, 78, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Barium, 56, 76, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Barium, 56, 78, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Barium, 56, 79, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Barium, 56, 80, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Barium, 56, 81, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Barium, 56, 82, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lanthanum, 57, 82, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cerium, 58, 78, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cerium, 58, 80, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cerium, 58, 82, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Cerium, 58, 84, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Praseodymium, 59, 82, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neodymium, 60, 82, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neodymium, 60, 83, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neodymium, 60, 85, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neodymium, 60, 86, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Neodymium, 60, 88, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Samarium, 62, 82, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Samarium, 62, 87, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Samarium, 62, 88, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Samarium, 62, 90, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Samarium, 62, 92, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Europium, 63, 90, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gadolinium, 64, 90, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gadolinium, 64, 91, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gadolinium, 64, 92, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gadolinium, 64, 93, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gadolinium, 64, 94, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gadolinium, 64, 96, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Terbium, 65, 94, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Dysprosium, 66, 90, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Dysprosium, 66, 92, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Dysprosium, 66, 94, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Dysprosium, 66, 95, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Dysprosium, 66, 96, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Dysprosium, 66, 97, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Dysprosium, 66, 98, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Holmium, 67, 98, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Erbium, 68, 94, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Erbium, 68, 96, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Erbium, 68, 98, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Erbium, 68, 99, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Erbium, 68, 100, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Erbium, 68, 102, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Thulium, 69, 100, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ytterbium, 70, 98, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ytterbium, 70, 100, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ytterbium, 70, 101, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ytterbium, 70, 102, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ytterbium, 70, 103, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ytterbium, 70, 104, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Ytterbium, 70, 106, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lutetium, 71, 104, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Hafnium, 72, 104, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Hafnium, 72, 105, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Hafnium, 72, 106, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Hafnium, 72, 107, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Hafnium, 72, 108, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Meitnerium, 73, 107, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tantalum, 73, 108, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tungsten, 74, 108, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tungsten, 74, 109, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tungsten, 74, 110, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Tungsten, 74, 112, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Rhenium, 75, 110, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Osmium, 76, 108, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Osmium, 76, 111, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Osmium, 76, 112, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Osmium, 76, 113, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Osmium, 76, 114, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Osmium, 76, 116, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Iridium, 77, 114, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Iridium, 77, 116, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Platinum, 78, 114, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Platinum, 78, 116, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Platinum, 78, 117, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Platinum, 78, 118, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Platinum, 78, 120, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Gold, 79, 118, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Mercury, 80, 116, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Mercury, 80, 118, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Mercury, 80, 119, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Mercury, 80, 120, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Mercury, 80, 121, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Mercury, 80, 122, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Mercury, 80, 124, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Thallium, 81, 122, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Thallium, 81, 124, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lead, 82, 122, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lead, 82, 124, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lead, 82, 125, IsotopeStability.Stable),
            new IsotopeInfo(ElementName.Lead, 82, 126, IsotopeStability.Stable)
        };
        public static List<IsotopeInfo> GetStableIsotopeOf(ElementName element)
        {
            return StableIsotopes.Where(ii => ii.ElementName == element).ToList();
        }
    }

    public class IsotopeInfo
    {
        public IsotopeInfo(ElementName elementName, int protons, int neutrons, IsotopeStability stability)
        {
            ElementName = elementName;
            Protons = protons;
            Neutrons = neutrons;
            Stability = stability;
        }

        public ElementName ElementName { get; }
        public int Protons { get; }
        public int Neutrons { get; }
        public IsotopeStability Stability { get; }


    }

    public enum IsotopeStability
    {
        Unknown,
        Stable
    }
}
