namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class DataSeriesEncodingMap
    {
        public CramEncoding BamBitFlagEncoding { get; set; }
        public CramEncoding CramBitFlagEncoding { get; set; }
        public CramEncoding ReferenceIdEncoding { get; set; }
        public CramEncoding ReadLengthsEncoding { get; set; }
        public CramEncoding InSeqPositionsEncoding { get; set; }
        public CramEncoding ReadGroupsEncoding { get; set; }
        public CramEncoding ReadNamesEncoding { get; set; }
        public CramEncoding NextMateBitFlagEncoding { get; set; }
        public CramEncoding NextFragmentReferenceSequenceIdEncoding { get; set; }
        public CramEncoding NextMateAlignmentStartEncoding { get; set; }
        public CramEncoding TemplateSizeEncoding { get; set; }
        public CramEncoding DistanceToNextFragmentEncoding { get; set; }
        public CramEncoding TagIdEncoding { get; set; }
        public CramEncoding NumberOfReadFeaturesEncoding { get; set; }
        public CramEncoding ReadFeaturesCodesEncoding { get; set; }
        public CramEncoding InReadPositionsEncoding { get; set; }
        public CramEncoding DeletionLengthEncoding { get; set; }
        public CramEncoding StretchesOfBasesEncoding { get; set; }
        public CramEncoding StretchesOfQualityScoresEncoding { get; set; }
        public CramEncoding BaseSubstitutionCodesEncoding { get; set; }
        public CramEncoding InsertionEncoding { get; set; }
        public CramEncoding ReferenceSkipLengthEncoding { get; set; }
        public CramEncoding PaddingEncoding { get; set; }
        public CramEncoding HardClipEncoding { get; set; }
        public CramEncoding SoftClipEncoding { get; set; }
        public CramEncoding MappingQualitiesEncoding { get; set; }
        public CramEncoding BasesEncoding { get; set; }
        public CramEncoding QualityScoresEncoding { get; set; }
    }
}