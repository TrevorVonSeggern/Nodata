using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Graph.Base
{
    public class ProbalisticCollectionFilter : ProbalisticCollectionFilter<object>, IProbabilisticDataScructure
    {
        public ProbalisticCollectionFilter() : base()
        {
        }
    }
    public class ProbalisticCollectionFilter<T> : IProbabilisticDataScructure<T> where T : class
    {
        private List<T> list = new List<T>();
        public ProbalisticCollectionFilter() { }
        public void AddItem(T item) => list.Add(item);

        public object Clone()
        {
            return new ProbalisticCollectionFilter<T> { list = list };
        }

        public bool PossiblyExists(T item) => list.Contains(item);
    }
}
