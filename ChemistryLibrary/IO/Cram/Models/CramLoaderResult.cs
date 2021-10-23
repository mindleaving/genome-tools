using System.Collections.Generic;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class CramLoaderResult 
    {
        public CramHeader CramHeader { get; }
        public List<CramDataContainer> DataContainers { get; }

        public CramLoaderResult(CramHeader cramHeader, List<CramDataContainer> dataContainers)
        {
            CramHeader = cramHeader;
            DataContainers = dataContainers;
        }
    }
}