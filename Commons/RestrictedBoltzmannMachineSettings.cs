namespace Commons
{
    public class RestrictedBoltzmannMachineSettings
    {
        public int InputNodes { get; set; }
        public int HiddenNodes { get; set; }
        public int TrainingIterations { get; set; }
        public double LearningRate { get; set; }
    }
}