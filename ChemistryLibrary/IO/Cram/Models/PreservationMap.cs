using System;
using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class PreservationMap
    {
        public bool ReadNames { get; }
        public bool ApDataSeriesDelta { get; }
        public bool ReferenceRequired { get; }
        public byte[] SubstitutionMatrix { get; }
        public List<List<TagId>> TagIdCombinations { get; }

        public PreservationMap()
        {
            ReadNames = true;
            ApDataSeriesDelta = true;
            ReferenceRequired = true;
            SubstitutionMatrix = Array.Empty<byte>();
            TagIdCombinations = new List<List<TagId>>();
        }

        public PreservationMap(
            bool readNames, bool apDataSeriesDelta, bool referenceRequired,
            byte[] substitutionMatrix, List<List<TagId>> tagIdCombinations)
        {
            ReadNames = readNames;
            ApDataSeriesDelta = apDataSeriesDelta;
            ReferenceRequired = referenceRequired;
            SubstitutionMatrix = substitutionMatrix;
            TagIdCombinations = tagIdCombinations;
        }
    }
}