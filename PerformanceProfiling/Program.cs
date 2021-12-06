using System.Threading.Tasks;
using GenomeTools.Studies;

namespace PerformanceProfiling
{
    public static class Program
    {
        public static async Task Main()
        {
            var wgsStudy = new WholeGenomeSequencingStudy();
            await wgsStudy.CallVariants("chr1");
        }
    }
}
