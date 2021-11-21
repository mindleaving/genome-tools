using GenomeTools.Tools;

namespace PerformanceProfiling
{
    public static class Program
    {
        public static void Main()
        {
            var vcfIndexBuilderRunner = new VcfIndexBuilderRunner();
            vcfIndexBuilderRunner.Run(@"F:\datasets\mygenome\OtherGenomes\genome-1000_chr6.vcf");
        }
    }
}
