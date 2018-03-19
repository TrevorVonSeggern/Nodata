using NoData.Graph.Base;
using NoData.Graph.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoData.Graph
{
    public class Tree : Base.Tree
    {
        public new Vertex Root => base.Root as Vertex;
        public new IEnumerable<Tuple<Edge, Tree>> Children => base.Children?.Cast<Tuple<Edge, Tree>>();

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }

        public void Traverse(Action<Edge> callback) => Traverse((IEdge iEdge) => callback(iEdge as Edge));
        public void Traverse(Action<Vertex> callback) => Traverse((IVertex iVertex) => callback(iVertex as Vertex));


        private void _Add_Initialize()
        {
            Root.Value.Initialize(() =>
            {
                var list = new List<string>(Root.Value.PropertyNames);
                list.AddRange(Children.Select(t => t.Item1.Value.PropertyName));
                return list;
            });
        }
        public void AddInstances(IEnumerable<object> instances)
        {
            if (instances == null)
                throw new ArgumentNullException(nameof(instances));
            _Add_Initialize();
            var info = Utility.ClassInfoCache.GetOrAdd(instances.GetType().GenericTypeArguments[0]);
            foreach (var instance in instances)
                _addInstance(instance, info);
        }

        public void AddInstance(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            _Add_Initialize();
            var info = Utility.ClassInfoCache.GetOrAdd(instance.GetType());
            _addInstance(instance, info);
        }

        private void _addInstance(object instance, Utility.ClassInfoUtility classInfo)
        {
            // add value.
            Root.Value.AddItem(instance);
            // add children.
            foreach(var child in Children)
            {
                var edge = child.Item1;
                var tree = child.Item2;
                var propertyName = edge.Value.PropertyName;
                var action = classInfo.AccessProperties[propertyName];
                if (edge.Value.IsCollection)
                    tree.AddInstances((IEnumerable<object>) action(instance));
                else
                    tree.AddInstance(action(instance));
            }
        }

        public override IGraph Flatten()
        {
            var edges = new List<Edge>();
            Traverse(edges.Add);
            var vertices = new Dictionary<Type, Vertex>();

            void MergeVertexes(IEnumerable<Vertex> selection)
            {
                foreach (var v in selection)
                {
                    if (!vertices.ContainsKey(v.Value.Type))
                    {
                        vertices.Add(v.Value.Type, v);
                        continue;
                    }
                    vertices[v.Value.Type].Merge(v);
                }
            }

            MergeVertexes(new[] { Root });
            MergeVertexes(edges.Select(e => e.From).Where(v => v != Root));
            MergeVertexes(edges.Select(e => e.To));
            return new Graph(vertices.Values.ToList(), edges);
        }
    }
}
