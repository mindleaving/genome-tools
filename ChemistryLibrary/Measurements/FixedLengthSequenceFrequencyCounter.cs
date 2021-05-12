using System.Linq;
using Commons.Collections;
using Commons.DataProcessing;

namespace GenomeTools.ChemistryLibrary.Measurements
{
    public class FixedLengthSequenceFrequencyCounter
    {
        public int SequenceLength { get; }
        private readonly CircularBuffer<char> circularBuffer;
        public CappedFrequencyCounter<string> FrequencyCounter { get; }
        public string CurrentSequence => new string(circularBuffer.ToArray());

        public FixedLengthSequenceFrequencyCounter(int sequenceLength, int combinationLimit)
        {
            this.SequenceLength = sequenceLength;
            circularBuffer = new CircularBuffer<char>(sequenceLength);
            FrequencyCounter = new CappedFrequencyCounter<string>(combinationLimit);
        }

        public void Add(char letter)
        {
            circularBuffer.Put(letter);
            if(circularBuffer.IsFilled)
                FrequencyCounter.Add(new string(circularBuffer.ToArray()));
        }

        public void ResetSequence()
        {
            circularBuffer.Clear();
        }
    }
}
