using GenomeTools.Studies;

namespace PerformanceProfiling
{
    public static class Program
    {
        public static void Main()
        {
            var testClass = new WholeGenomeSequencingStudy();
            testClass.GetReadsInRegion("chr1", 31244000, 31245000);
        }
    }
}
