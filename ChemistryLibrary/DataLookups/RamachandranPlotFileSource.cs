using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ChemistryLibrary.Extensions;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Objects;
using Commons.Extensions;
using Commons.Physics;

namespace ChemistryLibrary.DataLookups
{
    public class RamachandranPlotFileSource
    {
        private readonly Dictionary<AminoAcidName, RamachandranPlot> ramachandranPlots = new Dictionary<AminoAcidName, RamachandranPlot>();

        public RamachandranPlotFileSource(string distributionDirectory)
        {
            foreach (var aminoAcidName in EnumExtensions.GetValues<AminoAcidName>())
            {
                var distributionFilePath = Path.Combine(distributionDirectory, aminoAcidName.ToThreeLetterCode() + ".csv");
                var aminoAcidAngles = ParseRamachadranPlotFile(distributionFilePath);
                var ramachandranPlot = new RamachandranPlot(aminoAcidAngles);
                ramachandranPlots.Add(aminoAcidName, ramachandranPlot);
            }
        }

        public RamachandranPlot Get(AminoAcidName aminoAcidName)
        {
            return ramachandranPlots[aminoAcidName];
        }

        public static List<AminoAcidAngles> ParseRamachadranPlotFile(string distributionFilePath)
        {
            const char Delimiter = ';';

            var angles = new List<AminoAcidAngles>();
            using (var streamReader = new StreamReader(distributionFilePath))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if(line.StartsWith("#") || line.StartsWith("//"))
                        continue;
                    var splittedLine = line.Split(Delimiter);
                    if(splittedLine.Length != 3)
                        continue;
                    var omega = double.Parse(splittedLine[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                    var phi = double.Parse(splittedLine[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    var psi = double.Parse(splittedLine[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                    angles.Add(new AminoAcidAngles
                    {
                        Omega = omega.To(Unit.Degree),
                        Phi = phi.To(Unit.Degree),
                        Psi = psi.To(Unit.Degree)
                    });
                }
            }
            return angles;
        }
    }
}
