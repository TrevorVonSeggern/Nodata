using NoData.Graph.Base;
using NoData.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NoData.Graph
{
    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    public class Vertex : Vertex<ClassInfo>, ICloneable
    {
        public override ClassInfo Value { get; protected set; }

        public Vertex(ClassInfo value) : base(value) { }
        public Vertex(Type type) : base(new ClassInfo(type)) { }

        public override object Clone() => new Vertex(Value.Clone() as ClassInfo);

        private IEnumerable<MemberBinding> BindingsForProperties(IEnumerable<PropertyInfo> properties, Expression dto)
        {
            foreach(var prop in properties)
                yield return (Expression.Bind(prop, Expression.PropertyOrField(dto, prop.Name)));
        }

        #region Expressions for expansion

        public Expression GetExpandExpression(Expression dto, Tree tree) => BindingExpression(dto, ExpansionMemberBindings(dto, tree));

        private IEnumerable<MemberBinding> ExpansionMemberBindings(Expression dto, Tree tree)
        {
            var classInfo = ClassInfoCache.GetOrAdd(Value.Type);
            var outgoingEdges = tree.Children.Select(c => c.Item1);

            // Bind all the non-expandable types.
            foreach (var prop in BindingsForProperties(classInfo.NonExpandableProperties, dto))
                yield return prop;

            // navigation properties
            foreach (var prop in classInfo.NavigationProperties.Where(x => outgoingEdges.Any(c => c.Value.PropertyName == x.Name)))
            {
                var navigationVertex = outgoingEdges.FirstOrDefault(x => x.Value.PropertyName == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == navigationVertex).Item2;
                yield return (Expression.Bind(prop, navigationVertex.GetExpandExpression(Expression.PropertyOrField(dto, prop.Name), navigationTree)));
            }

            // collections
            foreach (var prop in classInfo.Collections.Where(x => outgoingEdges.Any(c => c.Value.PropertyName == x.Name)))
            {
                if (!prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Count() != 1)
                    continue;

                var collectionVertex = outgoingEdges.Single(x => x.Value.PropertyName == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == collectionVertex).Item2;
                var listType = typeof(List<>).MakeGenericType(collectionVertex.Value.Type);

                var childrenParameter = Expression.Parameter(collectionVertex.Value.Type, prop.Name);
                var childrenProperty = Expression.PropertyOrField(dto, prop.Name);
                var select = ExpressionBuilder.BuildSelectExpression(collectionVertex.Value.Type, 
                    childrenProperty, 
                    childrenParameter, collectionVertex.GetExpandExpression(childrenParameter, navigationTree));
                var list = Expression.Condition(
                    Expression.Equal(childrenProperty, Expression.Constant(null)),
                    Expression.New(listType.GetConstructor(new Type[] { }), new Expression[] { }),
                    Expression.New(listType.GetConstructor(new Type[] { listType }), select)
                    );
                yield return(Expression.Bind(prop, list));
            }
        }

        #endregion

        #region Expressions for select

        public Expression GetSelectExpression(Expression dto, Tree tree) =>  BindingExpression(dto, SelectionMemberBindings(dto, tree));

        private IEnumerable<MemberBinding> SelectionMemberBindings(Expression dto, Tree tree)
        {
            var classInfo = ClassInfoCache.GetOrAdd(Value.Type);
            var outgoingEdges = tree.Children.Select(c => c.Item1);

            foreach (var prop in BindingsForProperties(classInfo.NonExpandableProperties.Where(p => tree.Root.Value.GetSelectProperties().Contains(p.Name)), dto))
                yield return prop;

            // navigation properties
            foreach (var prop in classInfo.NavigationProperties.Where(x => outgoingEdges.Any(c => c.Value.PropertyName == x.Name)))
            {
                var navigationVertex = outgoingEdges.FirstOrDefault(x => x.Value.PropertyName == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == navigationVertex).Item2;
                yield return (Expression.Bind(prop, navigationVertex.GetSelectExpression(Expression.PropertyOrField(dto, prop.Name), navigationTree)));
            }

            // collections
            foreach (var prop in classInfo.Collections.Where(x => outgoingEdges.Any(c => c.Value.PropertyName == x.Name)))
            {
                if (!prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Count() != 1)
                    continue;

                var collectionVertex = outgoingEdges.Single(x => x.Value.PropertyName == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == collectionVertex).Item2;
                var listType = typeof(List<>).MakeGenericType(collectionVertex.Value.Type);

                var childrenParameter = Expression.Parameter(collectionVertex.Value.Type, prop.Name);
                var childrenProperty = Expression.PropertyOrField(dto, prop.Name);
                var select = ExpressionBuilder.BuildSelectExpression(collectionVertex.Value.Type,
                    childrenProperty,
                    childrenParameter, collectionVertex.GetSelectExpression(childrenParameter, navigationTree));
                var list = Expression.Condition(
                    Expression.Equal(childrenProperty, Expression.Constant(null)),
                    Expression.New(listType.GetConstructor(new Type[] { }), new Expression[] { }),
                    Expression.New(listType.GetConstructor(new Type[] { listType }), select)
                    );
                yield return (Expression.Bind(prop, list));
            }
        }

        #endregion

        protected Expression BindingExpression(Expression dto, IEnumerable<MemberBinding> bindings)
        {
            // TODO: might need for EF
            //var newDto = Expression.New(typeof(TDto).GetConstructors().Single(), new[] { dto }, members: memberBindings);
            var newDto = Expression.New(Value.Type);
            var selectExpression = Expression.MemberInit(newDto, bindings);

            return Expression.Condition(Expression.Equal(dto, Expression.Constant(null)),
                Expression.Constant(null, selectExpression.Type), selectExpression);
        }
    }

    /// <summary>
    /// A class representation in a graph context.
    /// </summary>
    internal class StatefulVertex
    {
        public enum StateType
        {
            UnReached,
            Discovered,
            Identified,
        }

        public StatefulVertex(Vertex vertex)
        {
            Vertex = vertex ?? throw new ArgumentNullException(nameof(vertex));
        }

        public readonly Vertex Vertex;

        public StateType Color = StateType.UnReached;
    }
}
