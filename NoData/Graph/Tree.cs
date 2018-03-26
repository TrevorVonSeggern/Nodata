using NoData.Graph.Base;
using NoData.Graph.Interfaces;
using NoData.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.Graph
{
    public class Tree : Base.Tree<Vertex, Edge, ClassInfo, EdgeMetaData>
    {
        public new Vertex Root => base.Root as Vertex;
        public new IEnumerable<Tuple<Edge, Tree>> Children => base.Children?.Cast<Tuple<Edge, Tree>>();

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }

        public static Tree CreateFromPathsTree(Vertex root, IEnumerable<Path> paths)
        {
            var c = new List<ITuple<Edge, Tree>>();
            paths = paths.Where(p => p.Edges.Count() > 0);
            if (paths.Count() != 0)
            {
                if (!paths.All(p => p.Edges.First().From.Value.Type == root.Value.Type))
                    throw new ArgumentException("Paths don't all begin at the same vertex");

                foreach (var path in paths.Select(p => p.Edges).GroupBy(x => x.First()))
                {
                    var childPaths = path.Select(p => new Path(p.Skip(1))).Where(p => p.Edges.Count() > 0);
                    var childRoot = path.Key.To.Clone() as Vertex;
                    var edge = new Edge(root, childRoot, path.First().First().Value);
                    c.Add(ITuple.Create(edge, CreateFromPathsTree(childRoot, childPaths)));
                }
            }
            return new Tree(root, c);
        }

        #region modifying the tree with instances
        private void _Add_Initialize()
        {
            Root.Value.Initialize(() =>
            {
                var list = new List<string>(NoData.Utility.ClassInfoCache.GetOrAdd(Root.Value.Type).NonExpandablePropertyNames);
                list.AddRange(Children.Select(t => t.Item1.Value.PropertyName));
                return list;
            });
        }

        public void AddInstances(IEnumerable<object> instances)
        {
            if (instances == null)
                return;
            _Add_Initialize();
            var info = ClassInfoCache.GetOrAdd(instances.GetType().GenericTypeArguments[0]);
            foreach (var instance in instances)
                _addInstance(instance, info);
        }

        public void AddInstance(object instance)
        {
            if (instance == null)
                return;
            _Add_Initialize();
            var info = NoData.Utility.ClassInfoCache.GetOrAdd(instance.GetType());
            _addInstance(instance, info);
        }

        private void _addInstance(object instance, ClassInfoUtility classInfo)
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

        #endregion

        #region building queryables
        public IQueryable<TDto> ApplyExpand<TDto>(IQueryable<TDto> query)
        {
            var parameter = Expression.Parameter(typeof(TDto));
            var body = Root?.GetExpandExpression(parameter, this) ?? throw new ArgumentNullException("Root filter node or resulting expression is null");

            var expr = ExpressionBuilder.BuildSelectExpression(query, parameter, body);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        public IQueryable<TDto> ApplyFilter<TDto>(IQueryable<TDto> query) where TDto : class, new()
        {
            var parameter = Expression.Parameter(typeof(TDto));
            var body = Root?.Value.FilterExpression;

            if (body is null)
                return query;

            var expr = ExpressionBuilder.BuildWhereExpression(query, parameter, body);

            return query.Provider.CreateQuery<TDto>(expr);
        }
        #endregion
    }
}
