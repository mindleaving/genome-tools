using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ChemistryLibrary;
using ChemistryLibrary.Measurements;
using ChemistryLibrary.Pdb;
using Commons;

namespace MoleculeViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty MoleculeViewModelProperty = DependencyProperty.Register("MoleculeViewModel",
            typeof(MoleculeViewModel), typeof(MainWindow), new PropertyMetadata(default(MoleculeViewModel)));
        public static readonly DependencyProperty SimulationViewModelProperty = DependencyProperty.Register("SimulationViewModel", 
            typeof(SimulationViewModel), typeof(MainWindow), new PropertyMetadata(default(SimulationViewModel)));

        public MainWindow()
        {
            InitializeComponent();

            //var water = MoleculeLibrary.H2O;
            //water.PositionAtoms();
            //var peptide = PeptideBuilder.PeptideFromString(
            //    "MQRSPLEKASVVSKLFFSWTRPILRKGYRQRLELSDIYQIPSVDSADNLSEKLEREWDRE"
            //    + "LASKKNPKLINALRRCFFWRFMFYGIFLYLGEVTKAVQPLLLGRIIASYDPDNKEERSIA"
            //    + "IYLGIGLCLLFIVRTLLLHPAIFGLHHIGMQMRIAMFSLIYKKTLKLSSRVLDKISIGQL"
            //    + "VSLLSNNLNKFDEGLALAHFVWIAPLQVALLMGLIWELLQASAFCGLGFLIVLALFQAGL"
            //    + "GRMMMKYRDQRAGKISERLVITSEMIENIQSVKAYCWEEAMEKMIENLRQTELKLTRKAA"
            //    + "YVRYFNSSAFFFSGFFVVFLSVLPYALIKGIILRKIFTTISFCIVLRMAVTRQFPWAVQT"
            //    + "WYDSLGAINKIQDFLQKQEYKTLEYNLTTTEVVMENVTAFWEEGFGELFEKAKQNNNNRK"
            //    + "TSNGDDSLFFSNFSLLGTPVLKDINFKIERGQLLAVAGSTGAGKTSLLMVIMGELEPSEG"
            //    + "KIKHSGRISFCSQFSWIMPGTIKENIIFGVSYDEYRYRSVIKACQLEEDISKFAEKDNIV"
            //    + "LGEGGITLSGGQRARISLARAVYKDADLYLLDSPFGYLDVLTEKEIFESCVCKLMANKTR"
            //    + "ILVTSKMEHLKKADKILILHEGSSYFYGTFSELQNLQPDFSSKLMGCDSFDQFSAERRNS"
            //    + "ILTETLHRFSLEGDAPVSWTETKKQSFKQTGEFGEKRKNSILNPINSIRKFSIVQKTPLQ"
            //    + "MNGIEEDSDEPLERRLSLVPDSEQGEAILPRISVISTGPTLQARRRQSVLNLMTHSVNQG"
            //    + "QNIHRKTTASTRKVSLAPQANLTELDIYSRRLSQETGLEISEEINEEDLKECFFDDMESI"
            //    + "PAVTTWNTYLRYITVHKSLIFVLIWCLVIFLAEVAASLVVLWLLGNTPLQDKGNSTHSRN"
            //    + "NSYAVIITSTSSYYVFYIYVGVADTLLAMGFFRGLPLVHTLITVSKILHHKMLHSVLQAP"
            //    + "MSTLNTLKAGGILNRFSKDIAILDDLLPLTIFDFIQLLLIVIGAIAVVAVLQPYIFVATV"
            //    + "PVIVAFIMLRAYFLQTSQQLKQLESEGRSPIFTHLVTSLKGLWTLRAFGRQPYFETLFHK"
            //    + "ALNLHTANWFLYLSTLRWFQMRIEMIFVIFFIAVTFISILTTGEGEGRVGIILTLAMNIM"
            //    + "STLQWAVNSSIDVDSLMRSVSRVFKFIDMPTEGKPTKSTKPYKNGQLSKVMIIENSHVKK"
            //    + "DDIWPSGGQMTVKDLTAKYTEGGNAILENISFSISPGQRVGLLGRTGSGKSTLLSAFLRL"
            //    + "LNTEGEIQIDGVSWDSITLQQWRKAFGVIPQKVFIFSGTFRKNLDPYEQWSDQEIWKVAD"
            //    + "EVGLRSVIEQFPGKLDFVLVDGGCVLSHGHKQLMCLARSVLSKAKILLLDEPSAHLDPVT"
            //    + "YQIIRRTLKQAFADCTVILCEHRIEAMLECQQFLVIEENKVRQYDSIQKLLNERSLFRQA"
            //    + "ISPSDRVKLFPHRNSSKCKSKPQIAALKEETEEEVQDTRL");
            //var aminoAcidBuilder = PeptideBuilder
            //    .PeptideFromString("IHTGEKPYKC");
            //var peptide = PeptideBuilder
            //    .PeptideFromString("Q");

            var filename = @"G:\Projects\HumanGenome\Protein-PDBs\5hhe.pdb";
            var result = PdbReader.ReadFile(filename);
            var peptide = result.Chains.First();
            var angleMeasurerer = new AminoAcidAngleMeasurer();
            var angleMeasurement = angleMeasurerer.MeasureAngles(peptide);
            var approximatePeptide = new ApproximatePeptide(peptide.AminoAcids.Select(aa => aa.Name).ToList());
            for (int aminoAcidIdx = 0; aminoAcidIdx < approximatePeptide.AminoAcids.Count; aminoAcidIdx++)
            {
                var aminoAcid = approximatePeptide.AminoAcids[aminoAcidIdx];
                var chainReference = peptide.AminoAcids[aminoAcidIdx];
                if (angleMeasurement.ContainsKey(chainReference))
                {
                    var angles = angleMeasurement[chainReference];
                    aminoAcid.OmegaAngle = angles.Omega;
                    aminoAcid.PhiAngle = angles.Phi;
                    aminoAcid.PsiAngle = angles.Psi;
                }
                else
                {
                    Debugger.Break();
                }
            }
            approximatePeptide.UpdatePositions();
            var approximatePeptideCompleter = new ApproximatePeptideCompleter(approximatePeptide);
            var backbone = approximatePeptideCompleter.GetFullPeptide();


            backbone.Molecule.MarkBackbone(backbone.MoleculeReference);
            backbone.Molecule.PositionAtoms(backbone.MoleculeReference.FirstAtomId, backbone.MoleculeReference.LastAtomId);

            var customForces = new List<CustomAtomForce>();
            //{
            //    new CustomAtomForce
            //    {
            //        AtomVertex = aminoAcidBuilder.FirstAtomId,
            //        ForceFunc = (atom,t) => 1e3*atom.Position.VectorTo(new Point3D(0,0,0))
            //    },
            //    new CustomAtomForce
            //    {
            //        AtomVertex = aminoAcidBuilder.LastAtomId,
            //        ForceFunc = (atom,t) => {
            //            if(t > 50.To(SIPrefix.Nano, Unit.Second))
            //                return new Vector3D(0,0,0);
            //            return 1e-4/(1 + t.In(SIPrefix.Pico, Unit.Second))*new Vector3D(1, 0, 0);
            //        }
            //    },
            //};

            //var moleculeReference = new MoleculeBuilder()
            //    .Start
            //    .Add(ElementName.Nitrogen)
            //    .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen);
            //var molecule = moleculeReference.Molecule;
            //molecule.PositionAtoms(moleculeReference.FirstAtomId, moleculeReference.LastAtomId);

            RedistributeCharges(backbone.Molecule);

            MoleculeViewModel = new MoleculeViewModel(backbone.Molecule);
            SimulationViewModel = new SimulationViewModel(MoleculeViewModel, customForces);

            //SimulationViewModel.RunSimulation();
            //SimulationViewModel.RunSimulation().ContinueWith(x => 
            //    File.WriteAllText(@"G:\Projects\HumanGenome\cftr.pdb", PdbSerializer.Serialize(peptide, "cftr")));
        }

        private void RedistributeCharges(Molecule aminoAcid)
        {
            var newCharges = aminoAcid.MoleculeStructure.Vertices
                .ToDictionary(vId => vId.Key, vertex => ((Atom) vertex.Value.Object).EffectiveCharge);
            double chargeChange;
            do
            {
                var currentCharges = newCharges;
                var vertices = aminoAcid.MoleculeStructure.Vertices
                    .Select(v => v.Value);
                foreach (var vertex in vertices)
                {
                    var neighbors = GraphAlgorithms.GetAdjacentVertices(aminoAcid.MoleculeStructure, vertex)
                        .Select(v => (Atom)v.Object)
                        .ToList();
                    var atoms = new[] { (Atom)vertex.Object }.Concat(neighbors).ToList();
                    var chargeSum = atoms.Sum(x => x.EffectiveCharge.In(Unit.ElementaryCharge))
                        - atoms.Count;
                    var electroNegativitySum = atoms.Sum(x => x.ElectroNegativity);
                    foreach (var atom in atoms)
                    {
                        atom.EffectiveCharge = (1+chargeSum*(atom.ElectroNegativity/electroNegativitySum)).To(Unit.ElementaryCharge);
                    }
                }
                newCharges = aminoAcid.MoleculeStructure.Vertices
                    .ToDictionary(vId => vId.Key, vertex => ((Atom) vertex.Value.Object).EffectiveCharge);
                chargeChange = aminoAcid.MoleculeStructure.Vertices.Keys
                    .Select(vId => (newCharges[vId] - currentCharges[vId]).Abs().In(Unit.ElementaryCharge))
                    .Max();

                //var effectiveCharges = aminoAcid.Atoms.OrderBy(atom => atom.EffectiveCharge)
                //    .Select(atom => new { Atom = atom, Charge = atom.EffectiveCharge.In(Unit.ElementaryCharge) })
                //    .ToList();
            } while (chargeChange > 0.01);
        }

        public SimulationViewModel SimulationViewModel
        {
            get { return (SimulationViewModel)GetValue(SimulationViewModelProperty); }
            set { SetValue(SimulationViewModelProperty, value); }
        }
        public MoleculeViewModel MoleculeViewModel
        {
            get { return (MoleculeViewModel) GetValue(MoleculeViewModelProperty); }
            set { SetValue(MoleculeViewModelProperty, value); }
        }
    }
}
