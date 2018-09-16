namespace Studies
{
    public class AlignedPair<T>
    {
        public AlignedPair(T item1, int item1Index, T item2, int item2Index)
        {
            Item1 = item1;
            Item2 = item2;
            Item1Index = item1Index;
            Item2Index = item2Index;
        }

        public int Item1Index { get; }
        public T Item1 { get; }

        public int Item2Index { get; }
        public T Item2 { get; }
    }
}