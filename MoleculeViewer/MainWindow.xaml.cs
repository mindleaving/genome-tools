using System.Collections.Generic;
using System.Windows;
using ChemistryLibrary;
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
            //var aminoAcid = PeptideBuilder.PeptideFromString(
            //        "MQRSPLEKASVVSKLFFSWTRPILRKGYRQRLELSDIYQIPSVDSADNLSEKLEREWDRE"
            //        + "LASKKNPKLINALRRCFFWRFMFYGIFLYLGEVTKAVQPLLLGRIIASYDPDNKEERSIA"
            //        + "IYLGIGLCLLFIVRTLLLHPAIFGLHHIGMQMRIAMFSLIYKKTLKLSSRVLDKISIGQL"
            //        + "VSLLSNNLNKFDEGLALAHFVWIAPLQVALLMGLIWELLQASAFCGLGFLIVLALFQAGL"
            //        + "GRMMMKYRDQRAGKISERLVITSEMIENIQSVKAYCWEEAMEKMIENLRQTELKLTRKAA"
            //        + "YVRYFNSSAFFFSGFFVVFLSVLPYALIKGIILRKIFTTISFCIVLRMAVTRQFPWAVQT"
            //        + "WYDSLGAINKIQDFLQKQEYKTLEYNLTTTEVVMENVTAFWEEGFGELFEKAKQNNNNRK"
            //        + "TSNGDDSLFFSNFSLLGTPVLKDINFKIERGQLLAVAGSTGAGKTSLLMVIMGELEPSEG"
            //        + "KIKHSGRISFCSQFSWIMPGTIKENIIFGVSYDEYRYRSVIKACQLEEDISKFAEKDNIV"
            //        + "LGEGGITLSGGQRARISLARAVYKDADLYLLDSPFGYLDVLTEKEIFESCVCKLMANKTR"
            //        + "ILVTSKMEHLKKADKILILHEGSSYFYGTFSELQNLQPDFSSKLMGCDSFDQFSAERRNS"
            //        + "ILTETLHRFSLEGDAPVSWTETKKQSFKQTGEFGEKRKNSILNPINSIRKFSIVQKTPLQ"
            //        + "MNGIEEDSDEPLERRLSLVPDSEQGEAILPRISVISTGPTLQARRRQSVLNLMTHSVNQG"
            //        + "QNIHRKTTASTRKVSLAPQANLTELDIYSRRLSQETGLEISEEINEEDLKECFFDDMESI"
            //        + "PAVTTWNTYLRYITVHKSLIFVLIWCLVIFLAEVAASLVVLWLLGNTPLQDKGNSTHSRN"
            //        + "NSYAVIITSTSSYYVFYIYVGVADTLLAMGFFRGLPLVHTLITVSKILHHKMLHSVLQAP"
            //        + "MSTLNTLKAGGILNRFSKDIAILDDLLPLTIFDFIQLLLIVIGAIAVVAVLQPYIFVATV"
            //        + "PVIVAFIMLRAYFLQTSQQLKQLESEGRSPIFTHLVTSLKGLWTLRAFGRQPYFETLFHK"
            //        + "ALNLHTANWFLYLSTLRWFQMRIEMIFVIFFIAVTFISILTTGEGEGRVGIILTLAMNIM"
            //        + "STLQWAVNSSIDVDSLMRSVSRVFKFIDMPTEGKPTKSTKPYKNGQLSKVMIIENSHVKK"
            //        + "DDIWPSGGQMTVKDLTAKYTEGGNAILENISFSISPGQRVGLLGRTGSGKSTLLSAFLRL"
            //        + "LNTEGEIQIDGVSWDSITLQQWRKAFGVIPQKVFIFSGTFRKNLDPYEQWSDQEIWKVAD"
            //        + "EVGLRSVIEQFPGKLDFVLVDGGCVLSHGHKQLMCLARSVLSKAKILLLDEPSAHLDPVT"
            //        + "YQIIRRTLKQAFADCTVILCEHRIEAMLECQQFLVIEENKVRQYDSIQKLLNERSLFRQA"
            //        + "ISPSDRVKLFPHRNSSKCKSKPQIAALKEETEEEVQDTRL")
            //    .Molecule;
            var aminoAcidBuilder = PeptideBuilder
                .PeptideFromString("IHTGEKPYKC");
            //var aminoAcidBuilder = AminoAcidLibrary.Glycine
            //    .Add(AminoAcidLibrary.Threonine)
            //    .Add(AminoAcidLibrary.Glycine);
            var aminoAcid = aminoAcidBuilder.Molecule;
            aminoAcid.PositionAtoms();

            var customForces = new List<CustomAtomForce>
            {
                new CustomAtomForce
                {
                    AtomVertex = aminoAcidBuilder.FirstAtomId,
                    ForceFunc = (atom,t) => 1e3*atom.Position.VectorTo(new Point3D(0,0,0))
                },
                new CustomAtomForce
                {
                    AtomVertex = aminoAcidBuilder.LastAtomId,
                    ForceFunc = (atom,t) => {
                        if(t > 50.To(SIPrefix.Nano, Unit.Second))
                            return new Vector3D(0,0,0);
                        return 1e-4/(1 + t.In(SIPrefix.Pico, Unit.Second))*new Vector3D(1, 0, 0);
                    }
                },
            };

            //var molecule = new MoleculeBuilder()
            //    .Start
            //    .Add(ElementName.Nitrogen)
            //    .AddToCurrentAtom(ElementName.Hydrogen, ElementName.Hydrogen, ElementName.Hydrogen)
            //    .Molecule;
            //molecule.PositionAtoms();

            MoleculeViewModel = new MoleculeViewModel(aminoAcid);
            SimulationViewModel = new SimulationViewModel(MoleculeViewModel, customForces);

            SimulationViewModel.RunSimulation();
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
