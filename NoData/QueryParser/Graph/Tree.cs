﻿using NoData.Graph.Base;
using NoData.QueryParser.ParsingTools.Groupers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.QueryParser.Graph
{
    public class Tree : Tree<Vertex, Edge, TextInfo, EdgeInfo>, IRepresent
    {
        public override Vertex Root { get; protected set; }
        public new IEnumerable<ITuple<Edge, Tree>> Children => base.Children?.Cast<ITuple<Edge, Tree>>();
        public string Representation => Root.Value.Representation;

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }


        public override string ToString() => Root.ToString() + " ";



    }
}
