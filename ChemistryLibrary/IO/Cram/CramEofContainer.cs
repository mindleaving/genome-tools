using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram
{
    public class CramEofContainer : CramDataContainer
    {
        public CramEofContainer()
            : base(
                new CramContainerHeader(15, -1, 4542278, 0, 0, 0, 0, 1, 1339669765, new List<int>()),
                new CramCompressionHeader(new PreservationMap(), new DataSeriesEncodingMap(), new TagEncodingMap(), 1258382318),
                new List<CramSlice>())
        {
        }
    }
}