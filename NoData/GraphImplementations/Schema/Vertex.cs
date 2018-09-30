using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CodeTools;
using Graph.Interfaces;
using NoData.Utility;

namespace NoData.GraphImplementations.Schema
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    [Immutable]
    public class Vertex : Graph.Vertex<ClassInfo>
    {
        public Vertex(ClassInfo value) : base(value) { }
    }
}
