using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class ReferenceSequenceSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.ReferenceSequence;
        public string ReferenceSequenceName { get; }
        public uint ReferenceSequenceLength { get; }
        public string AlternativeLocus { get; }
        public List<string> AlternativeNames { get; }
        public string GenomeAssemblyId { get; }
        public string Description { get; }
        public string Md5Checksum { get; }
        public string Species { get; }
        public string MoleculeTopology { get; }
        public string StorageLocation { get; }

        public ReferenceSequenceSamHeaderEntry(
            string referenceSequenceName, uint referenceSequenceLength, string alternativeLocus,
            List<string> alternativeNames, string genomeAssemblyId, string description,
            string md5Checksum, string species, string moleculeTopology,
            string storageLocation)
        {
            ReferenceSequenceName = referenceSequenceName;
            ReferenceSequenceLength = referenceSequenceLength;
            AlternativeLocus = alternativeLocus;
            AlternativeNames = alternativeNames ?? new List<string>();
            GenomeAssemblyId = genomeAssemblyId;
            Description = description;
            Md5Checksum = md5Checksum;
            Species = species;
            MoleculeTopology = moleculeTopology;
            StorageLocation = storageLocation;
        }
    }
}