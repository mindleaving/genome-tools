using System;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramFileDefinition
    {
        public Version Version { get; }
        public byte[] FileId { get; }

        public CramFileDefinition(Version version, byte[] fileId)
        {
            Version = version;
            FileId = fileId;
        }
    }
}