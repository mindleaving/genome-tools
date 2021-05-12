using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commons.Extensions;
using NUnit.Framework;

namespace GenomeTools.Studies
{
    [TestFixture]
    public class CftrVariantsStudy
    {
        [Test]
        public void ReadMutationPosition()
        {
            var mutationNameFile = @"F:\HumanGenome\cftr_mutations.csv";
            var lines = File.ReadAllLines(mutationNameFile);
            //var mutationPositions = new List<string>();
            var heatMap = new Dictionary<int, int>();
            foreach (var line in lines)
            {
                var splittetLine = line.Split(',');
                var cDnaName = splittetLine[0];
                var mutationCount = int.Parse(splittetLine[1]);
                var positionString = new string(line.Substring(2).TakeWhile(char.IsDigit).ToArray());
                if(!int.TryParse(positionString, out var mutationPosition))
                    continue;
                var mutationDescription = new string(line.Substring(2).SkipWhile(char.IsDigit).ToArray());
                var nonCoding = mutationDescription[0].InSet('-', '+');
                //mutationPositions.Add($"{line};{mutationPosition};{nonCoding}");
                if(!heatMap.ContainsKey(mutationPosition))
                    heatMap.Add(mutationPosition, 0);
                heatMap[mutationPosition] += mutationCount;
            }
            File.WriteAllLines(mutationNameFile.Replace(".csv", "_positions.csv"), heatMap.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key};{kvp.Value}"));
        }

        [Test]
        public void SequenceSimilarity()
        {
            var cftrSequence = "MQRSPLEKASVVSKLFFSWTRPILRKGYRQRLELSDIYQIPSVDSADNLSEKLEREWDRELASKKNPKLINALRRCFFWRFMFYGIFLYLGEVTKAVQPLLLGRIIASYDPDNKEERSIAIYLGIGLCLLFIVRTLLLHPAIFGLHHIGMQMRIAMFSLIYKKTLKLSSRVLDKISIGQLVSLLSNNLNKFDEGLALAHFVWIAPLQVALLMGLIWELLQASAFCGLGFLIVLALFQAGLGRMMMKYRDQRAGKISERLVITSEMIENIQSVKAYCWEEAMEKMIENLRQTELKLTRKAAYVRYFNSSAFFFSGFFVVFLSVLPYALIKGIILRKIFTTISFCIVLRMAVTRQFPWAVQTWYDSLGAINKIQDFLQKQEYKTLEYNLTTTEVVMENVTAFWEEGFGELFEKAKQNNNNRKTSNGDDSLFFSNFSLLGTPVLKDINFKIERGQLLAVAGSTGAGKTSLLMVIMGELEPSEGKIKHSGRISFCSQFSWIMPGTIKENIIFGVSYDEYRYRSVIKACQLEEDISKFAEKDNIVLGEGGITLSGGQRARISLARAVYKDADLYLLDSPFGYLDVLTEKEIFESCVCKLMANKTRILVTSKMEHLKKADKILILHEGSSYFYGTFSELQNLQPDFSSKLMGCDSFDQFSAERRNSILTETLHRFSLEGDAPVSWTETKKQSFKQTGEFGEKRKNSILNPINSIRKFSIVQKTPLQMNGIEEDSDEPLERRLSLVPDSEQGEAILPRISVISTGPTLQARRRQSVLNLMTHSVNQGQNIHRKTTASTRKVSLAPQANLTELDIYSRRLSQETGLEISEEINEEDLKECFFDDMESIPAVTTWNTYLRYITVHKSLIFVLIWCLVIFLAEVAASLVVLWLLGNTPLQDKGNSTHSRNNSYAVIITSTSSYYVFYIYVGVADTLLAMGFFRGLPLVHTLITVSKILHHKMLHSVLQAPMSTLNTLKAGGILNRFSKDIAILDDLLPLTIFDFIQLLLIVIGAIAVVAVLQPYIFVATVPVIVAFIMLRAYFLQTSQQLKQLESEGRSPIFTHLVTSLKGLWTLRAFGRQPYFETLFHKALNLHTANWFLYLSTLRWFQMRIEMIFVIFFIAVTFISILTTGEGEGRVGIILTLAMNIMSTLQWAVNSSIDVDSLMRSVSRVFKFIDMPTEGKPTKSTKPYKNGQLSKVMIIENSHVKKDDIWPSGGQMTVKDLTAKYTEGGNAILENISFSISPGQRVGLLGRTGSGKSTLLSAFLRLLNTEGEIQIDGVSWDSITLQQWRKAFGVIPQKVFIFSGTFRKNLDPYEQWSDQEIWKVADEVGLRSVIEQFPGKLDFVLVDGGCVLSHGHKQLMCLARSVLSKAKILLLDEPSAHLDPVTYQIIRRTLKQAFADCTVILCEHRIEAMLECQQFLVIEENKVRQYDSIQKLLNERSLFRQAISPSDRVKLFPHRNSSKCKSKPQIAALKEETEEEVQDTRL";
            var reversedSequence = new string(cftrSequence.Reverse().ToArray());
            Console.WriteLine(reversedSequence);

            var nbd1 = "KTSNGDDSLFFSNFSLLGTPVLKDINFKIERGQLLAVAGSTGAGKTSLLMVIMGELEPSEGKIKHSGRISFCSQFSWIMPGTIKENIIFGVSYDEYRYRSVIKACQ";
            var nbd2 = "SGGQMTVKDLTAKYTEGGNAILENISFSISPGQRVGLLGRTGSGKSTLLSAFLRLLNTEGEIQIDGVSWDSITLQQWRKAFGVIPQKVFIFSGTFRKNLDPYEQWS";

        }
    }
}
