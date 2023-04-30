namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    public class ProgramSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.Program;
        public string ProgramId { get; }
        public string ProgramName { get; }
        public string CommandLine { get; }
        public string PreviousProgramId { get; }
        public string Description { get; }
        public string ProgramVersion { get; }

        public ProgramSamHeaderEntry(
            string programId, string programName, string commandLine,
            string previousProgramId, string description, string programVersion)
        {
            ProgramId = programId;
            ProgramName = programName;
            CommandLine = commandLine;
            PreviousProgramId = previousProgramId;
            Description = description;
            ProgramVersion = programVersion;
        }
    }
}