using System;

namespace GenomeTools.ChemistryLibrary.IO.Sam
{
    internal class ReadGroupSamHeaderEntry : SamHeaderEntry
    {
        public override HeaderEntryType Type => HeaderEntryType.ReadGroup;
        public string ReadGroupId { get; }
        public string Barcode { get; }
        public string SequencingCenter { get; }
        public string Description { get; }
        public DateTime? Date { get; }
        public string FlowOrder { get; }
        public string KeySequence { get; }
        public string Library { get; }
        public string Programs { get; }
        public string PredictedMedianInsertSize { get; }
        public string Platform { get; }
        public string PlatformModel { get; }
        public string PlatformUnit { get; }
        public string Sample { get; }

        public ReadGroupSamHeaderEntry(
            string readGroupId, string barcode, string sequencingCenter,
            string description, DateTime? date, string flowOrder,
            string keySequence, string library, string programs,
            string predictedMedianInsertSize, string platform, string platformModel,
            string platformUnit, string sample)
        {
            ReadGroupId = readGroupId;
            Barcode = barcode;
            SequencingCenter = sequencingCenter;
            Description = description;
            Date = date;
            FlowOrder = flowOrder;
            KeySequence = keySequence;
            Library = library;
            Programs = programs;
            PredictedMedianInsertSize = predictedMedianInsertSize;
            Platform = platform;
            PlatformModel = platformModel;
            PlatformUnit = platformUnit;
            Sample = sample;
        }
    }
}