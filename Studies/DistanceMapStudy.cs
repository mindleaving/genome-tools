using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using NUnit.Framework;

namespace Studies
{
    [TestFixture]
    public class DistanceMapStudy
    {
        [Test]
        public void ReconstructProteinFromDistanceMap()
        {
            var distanceMapPath = @"F:\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions\DistanceMaps\pdb1a2b_model000.png";
            var outputPath = @"F:\HumanGenome\Protein-PDBs\HumanProteins\AminoAcidPositions\DistanceMapManipulationStudy";
            var description = "blurred";
            var outputFilenameBase = Path.GetFileNameWithoutExtension(distanceMapPath) + "_" + description;
            var distanceMap = new Image<Gray, byte>(distanceMapPath);
            var blurredDistanceMap = new Image<Gray, byte>(distanceMap.Size);
            CvInvoke.GaussianBlur(distanceMap, blurredDistanceMap, new Size(5, 5), 3);
            blurredDistanceMap.Save(Path.Combine(outputPath, outputFilenameBase + ".png"));
            var aminoAcidPositions = DistanceMapTo3DCalculator.Calculate(blurredDistanceMap);
            var lines = aminoAcidPositions.Select(p => p.ToString());
            File.WriteAllLines(Path.Combine(outputPath, outputFilenameBase + ".csv"), lines);
        }
    }
}
