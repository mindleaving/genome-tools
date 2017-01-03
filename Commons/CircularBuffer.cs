using System.Collections;
using System.Collections.Generic;

namespace Commons
{
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private readonly T[] buffer;
        private readonly int bufferSize;
        private int nextIndex = 0;
        private bool bufferFilled;

        public CircularBuffer(int bufferSize)
        {
            this.bufferSize = bufferSize;
            buffer = new T[bufferSize];
        }

        public bool IsFilled => bufferFilled;

        public T Put(T item)
        {
            var removedItem = buffer[nextIndex];
            buffer[nextIndex] = item;
            nextIndex++;
            if (nextIndex == bufferSize)
            {
                nextIndex = 0;
                if (!bufferFilled) bufferFilled = true; ;
            }
            return removedItem;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (bufferFilled)
            {
                for (var idx = nextIndex; idx < bufferSize; idx++)
                    yield return buffer[idx];
            }
            for (var idx = 0; idx < nextIndex; idx++)
                yield return buffer[idx];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            nextIndex = 0;
            bufferFilled = false;
        }
    }
}
