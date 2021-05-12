using System.Collections.Generic;
using System.Linq;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.Builders
{
    public static class AtomExtractor
    {
        public static List<Atom> FromMolecule(Molecule molecule)
        {
            return molecule.Atoms.ToList();
        }

        public static List<Atom> FromApproximatePeptide(ApproximatePeptide peptide)
        {
            var atoms = new List<Atom>();
            foreach (var aminoAcid in peptide.AminoAcids)
            {
                var nitrogenAtom = Atom.FromStableIsotope(ElementName.Nitrogen);
                nitrogenAtom.Position = aminoAcid.NitrogenPosition;
                var carbonAlphaAtom = Atom.FromStableIsotope(ElementName.Carbon);
                carbonAlphaAtom.Position = aminoAcid.CarbonAlphaPosition;
                var carbonAtom = Atom.FromStableIsotope(ElementName.Carbon);
                carbonAtom.Position = aminoAcid.CarbonPosition;
                atoms.Add(nitrogenAtom);
                atoms.Add(carbonAlphaAtom);
                atoms.Add(carbonAtom);
            }
            return atoms;
        }
    }
}
