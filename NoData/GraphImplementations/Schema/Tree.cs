﻿using Graph;
using Graph.Interfaces;
using NoData.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NoData.GraphImplementations.Schema
{
    public class Tree : Tree<Vertex, Edge, ClassInfo, EdgeMetaData>
    {
        public override Vertex Root { get; protected set; }
        public new IEnumerable<Tuple<Edge, Tree>> Children => base.Children?.Cast<Tuple<Edge, Tree>>();

        public Tree(Vertex root) : base(root, new List<ITuple<Edge, Tree>>()) { }
        public Tree(Vertex root, IEnumerable<ITuple<Edge, Tree>> children) : base(root, children) { }

        public Tree(Vertex root, IEnumerable<Path> expandPaths) : base(root, expandPaths)
        {
        }

        public static Tree CreateFromPathsTree(Vertex root, IEnumerable<Path> expandPaths, IEnumerable<PathToProperty> selections)
        {
            var c = new List<ITuple<Edge, Tree>>();
            expandPaths = expandPaths?.Where(p => p != null && p.Edges?.Any() == true) ?? new List<Path>();
            selections = selections ?? new List<PathToProperty>();
            if (expandPaths.Any())
            {
                if (!expandPaths.All(p => p.Edges.First().From.Value.Type == root.Value.Type) && !selections.All(p => p.Edges.First().From.Value.Type == root.Value.Type))
                    throw new ArgumentException("Paths don't all begin at the same vertex");

                foreach (var path in expandPaths.Select(p => p.Edges).GroupBy(x => x.First()))
                {
                    var childPaths = path.Select(p => new Path(p.Skip(1))).Where(p => p.Edges.Any());
                    var childRoot = path.Key.To.Clone() as Vertex;

                    var childSelectionsPaths = selections.Where(s => s.Edges.Any() && s.Edges.First().From.Value.Type == childRoot.Value.Type)
                        .Select(p => new PathToProperty(p.Edges.Skip(1), p.Property));

                    var edge = new Edge(root, childRoot, path.First().First().Value);
                    c.Add(ITuple.Create(edge, CreateFromPathsTree(childRoot, childPaths, childSelectionsPaths)));
                }
            }
            foreach (var propertyName in selections.Where(x => x is null || !x.Edges.Any()).Select(x => x.Property.Name))
                root.Value.AddSelection(propertyName);
            return new Tree(root, c);
        }


        public GraphSchema Flatten()
        {
            return (GraphSchema)this.Flatten<GraphSchema, Tree, Vertex, Edge, ClassInfo, EdgeMetaData>((vList, eList) => new GraphSchema(vList, eList));
        }

        #region modifying the tree with instances
        private void _Add_Initialize()
        {
            Root.Value.Initialize(() => Children.Select(t => t.Item1.Value.PropertyName));
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
            foreach (var child in Children)
            {
                var edge = child.Item1;
                var tree = child.Item2;
                var propertyName = edge.Value.PropertyName;
                var action = classInfo.AccessProperties[propertyName];
                if (edge.Value.IsCollection)
                    tree.AddInstances((IEnumerable<object>)action(instance));
                else
                    tree.AddInstance(action(instance));
            }
        }

        #endregion

        #region build queryables
        public IQueryable<TDto> ApplyExpand<TDto>(IQueryable<TDto> query, ParameterExpression parameter) where TDto : class, new()
        {
            var body = Root?.GetExpandExpression(parameter, this) ?? throw new ArgumentNullException("Root filter node or resulting expression is null");

            var expr = ExpressionBuilder.BuildSelectExpression(query, parameter, body);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        public IQueryable<TDto> ApplyFilter<TDto>(IQueryable<TDto> query, ParameterExpression parameter, Expression filterExpression) where TDto : class, new()
        {
            if (filterExpression is null) return query;

            var expr = ExpressionBuilder.BuildWhereExpression(query, parameter, filterExpression);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        public IQueryable<TDto> ApplySelect<TDto>(IQueryable<TDto> query, ParameterExpression parameter) where TDto : class, new()
        {
            var selectExpression = Root?.GetSelectExpression(parameter, this) ?? throw new ArgumentNullException("Root filter node or resulting expression is null");

            if (selectExpression is null) return query;

            var expr = ExpressionBuilder.BuildSelectExpression(query, parameter, selectExpression);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        #endregion
    }
}