using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramRawData 
    {
        public CramHeader CramHeader { get; }
        public List<CramDataContainer> DataContainers { get; }

        public CramRawData(CramHeader cramHeader, List<CramDataContainer> dataContainers)
        {
            CramHeader = cramHeader;
            DataContainers = dataContainers;
        }
    }
}