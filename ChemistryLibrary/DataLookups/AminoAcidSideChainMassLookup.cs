using System.Collections.Generic;
using Commons.Extensions;
using Commons.Physics;
using GenomeTools.ChemistryLibrary.Objects;

namespace GenomeTools.ChemistryLibrary.DataLookups
{
    public class AminoAcidSideChainMassLookup
    {
        public static Dictionary<AminoAcidName, UnitValue> SideChainMasses = new Dictionary<AminoAcidName, UnitValue>
        {
            {AminoAcidName.Alanine, GetMass(ElementName.Carbon) + 3*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Glycine, 0.To(Unit.Kilogram) },
            {AminoAcidName.Isoleucine, 4*GetMass(ElementName.Carbon) + 9*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Leucine, 4*GetMass(ElementName.Carbon) + 9*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Proline, 3*GetMass(ElementName.Carbon) + 6*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Valine, 3*GetMass(ElementName.Carbon) + 7*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Phenylalanine, 7*GetMass(ElementName.Carbon) + 7*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Tryptophan, 9*GetMass(ElementName.Carbon) + GetMass(ElementName.Nitrogen) + 7*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Tyrosine, 7*GetMass(ElementName.Carbon) + GetMass(ElementName.Oxygen) + 7*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.AsparticAcid, 2*GetMass(ElementName.Carbon) + 2*GetMass(ElementName.Oxygen) + 2*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.GlutamicAcid, 3*GetMass(ElementName.Carbon) + 2*GetMass(ElementName.Oxygen) + 4*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Arginine, 4*GetMass(ElementName.Carbon) + 3*GetMass(ElementName.Nitrogen) + 11*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Histidine, 4*GetMass(ElementName.Carbon) + 2*GetMass(ElementName.Nitrogen) + 6*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Lysine, 4*GetMass(ElementName.Carbon) + GetMass(ElementName.Nitrogen) + 11*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Serine, GetMass(ElementName.Carbon) + GetMass(ElementName.Oxygen) + 3*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Threonine, 2*GetMass(ElementName.Carbon) + GetMass(ElementName.Oxygen) + 5*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Cysteine, GetMass(ElementName.Carbon) + GetMass(ElementName.Sulfur) + 3*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Methionine, 3*GetMass(ElementName.Carbon) + GetMass(ElementName.Sulfur) + 7*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Asparagine, 2*GetMass(ElementName.Carbon) + GetMass(ElementName.Oxygen) + GetMass(ElementName.Nitrogen) + 4*GetMass(ElementName.Hydrogen) },
            {AminoAcidName.Glutamine, 3*GetMass(ElementName.Carbon) + GetMass(ElementName.Oxygen) + GetMass(ElementName.Nitrogen) + 6*GetMass(ElementName.Hydrogen) }
        };

        private static UnitValue GetMass(ElementName carbon)
        {
            return PeriodicTable.GetSingleAtomMass(carbon);
        }
    }
}
