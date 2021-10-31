using System.Collections.Generic;
using GenomeTools.ChemistryLibrary.IO.Cram.Encodings;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramEofContainer : CramDataContainer
    {
        public CramEofContainer()
            : base(
                new CramContainerHeader(15, -1, 4542278, 0, 0, 0, 0, 1, 1339669765, new List<int>()),
                new CramCompressionHeader(new PreservationMap(), new DataSeriesEncodingMap(), new Dictionary<TagId, ICramEncoding<byte[]>>(), 1258382318),
                new List<CramSlice>())
        {
        }
    }
}