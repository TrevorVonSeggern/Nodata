using System;
using System.Collections.Generic;
using System.Text;

namespace NoData.Graph.Base
{
    /// <summary>
    /// A wrapper around Tuple for gaining covariance
    /// </summary>
    public static class ITuple
    {
        private class _ITuple<T1> : Tuple<T1>, ITuple<T1> { public _ITuple(T1 item1) : base(item1) { } }
        public static ITuple<T1> Create<T1>(T1 item1) { return new _ITuple<T1>(item1); }

        private class _ITuple<T1, T2> : Tuple<T1, T2>, ITuple<T1, T2> { public _ITuple(T1 item1, T2 item2) : base(item1, item2) { } }
        public static ITuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) { return new _ITuple<T1, T2>(item1, item2); }

        private class _ITuple<T1, T2, T3> : Tuple<T1, T2, T3>, ITuple<T1, T2, T3> { public _ITuple(T1 item1, T2 item2, T3 item3) : base(item1, item2, item3) { } }
        public static ITuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) { return new _ITuple<T1, T2, T3>(item1, item2, item3); }
    }

    public interface ITuple<out T1> { T1 Item1 { get; } }
    public interface ITuple<out T1, out T2> { T1 Item1 { get; } T2 Item2 { get; } }
    public interface ITuple<out T1, out T2, out T3> { T1 Item1 { get; } T2 Item2 { get; } T3 Item3 { get; } }
}
