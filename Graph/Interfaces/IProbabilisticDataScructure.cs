using System;

namespace Graph.Interfaces
{
    public interface IProbabilisticDataScructure : IProbabilisticDataScructure<object> { }
    public interface IProbabilisticDataScructure<T> : ICloneable where T : class
    {
        void AddItem(T item);
        bool PossiblyExists(T item);
    }
}
