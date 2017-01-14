using System;
using System.Collections.Generic;

namespace Commons
{
    public interface IDataSource<out T> : IDisposable
    {
        void Reset();
        IEnumerable<T> GetNext();
    }
}