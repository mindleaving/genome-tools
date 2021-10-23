using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class DataSeriesEncodingMap
    {
        public ICramEncoding<int> BamBitFlagEncoding { get; set; }
        public ICramEncoding<int> CramBitFlagEncoding { get; set; }
        public ICramEncoding<int> ReferenceIdEncoding { get; set; }
        public ICramEncoding<int> ReadLengthsEncoding { get; set; }
        public ICramEncoding<int> InSeqPositionsEncoding { get; set; }
        public ICramEncoding<int> ReadGroupsEncoding { get; set; }
        public ICramEncoding<byte[]> ReadNamesEncoding { get; set; }
        public ICramEncoding<int> NextMateBitFlagEncoding { get; set; }
        public ICramEncoding<int> NextFragmentReferenceSequenceIdEncoding { get; set; }
        public ICramEncoding<int> NextMateAlignmentStartEncoding { get; set; }
        public ICramEncoding<int> TemplateSizeEncoding { get; set; }
        public ICramEncoding<int> DistanceToNextFragmentEncoding { get; set; }
        public ICramEncoding<int> TagIdEncoding { get; set; }
        public ICramEncoding<int> NumberOfReadFeaturesEncoding { get; set; }
        public ICramEncoding<byte> ReadFeaturesCodesEncoding { get; set; }
        public ICramEncoding<int> InReadPositionsEncoding { get; set; }
        public ICramEncoding<int> DeletionLengthEncoding { get; set; }
        public ICramEncoding<byte[]> StretchesOfBasesEncoding { get; set; }
        public ICramEncoding<byte[]> StretchesOfQualityScoresEncoding { get; set; }
        public ICramEncoding<byte> BaseSubstitutionCodesEncoding { get; set; }
        public ICramEncoding<byte[]> InsertionEncoding { get; set; }
        public ICramEncoding<int> ReferenceSkipLengthEncoding { get; set; }
        public ICramEncoding<int> PaddingEncoding { get; set; }
        public ICramEncoding<int> HardClipEncoding { get; set; }
        public ICramEncoding<byte[]> SoftClipEncoding { get; set; }
        public ICramEncoding<int> MappingQualitiesEncoding { get; set; }
        public ICramEncoding<byte> BasesEncoding { get; set; }
        public ICramEncoding<byte> QualityScoresEncoding { get; set; }
    }
}