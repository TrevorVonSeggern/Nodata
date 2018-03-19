using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Graph.Base
{
    public interface IProbabilisticDataScructure : IProbabilisticDataScructure<object> { }
    public interface IProbabilisticDataScructure<T> : ICloneable where T : class
    {
        void AddItem(T item);
        bool PossiblyExists(T item);
    }
}
