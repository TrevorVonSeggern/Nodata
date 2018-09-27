using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Cache;
using NoData.GraphImplementations.Schema;

namespace NoData.Utility
{
    internal static class ExpressionBuilder
    {
        public static Expression BuildSelectExpression<TDto>(IQueryable<TDto> query, ParameterExpression parameter, Expression body)
        {
            return Expression.Call(
                typeof(Queryable), "Select", new[] { typeof(TDto), typeof(TDto) },
                query.Expression,
                Expression.Lambda<Func<TDto, TDto>>(body, parameter)
            );
        }

        public static Expression BuildSelectExpression<TDto>(Expression enumerable, ParameterExpression parameter, Expression body)
        {
            return Expression.Call(
                typeof(Enumerable), "Select", new[] { typeof(TDto), typeof(TDto) },
                enumerable,
                Expression.Lambda<Func<TDto, TDto>>(body, parameter)
            );
        }

        public static Expression BuildSelectExpression(Type type, Expression enumerable, ParameterExpression parameter, Expression body)
        {
            return GenericHelper.CreateAndCallMethodOnStaticClass(
                typeof(ExpressionBuilder),
                new[] { type },
                nameof(BuildSelectExpression),
                new[] { typeof(Expression), typeof(ParameterExpression), typeof(Expression) },
                new object[] { enumerable, parameter, body }) as Expression;
        }

        public static Expression BuildWhereExpression<TDto>(IQueryable<TDto> query, ParameterExpression parameter, Expression body)
        {
            return Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { typeof(TDto) },
                query.Expression,
                Expression.Lambda(body, parameter)
            );
        }



        #region build queryables
        public static IQueryable<TDto> ApplyExpand<TDto>(Tree tree, IQueryable<TDto> query, ParameterExpression parameter, IClassCache cache) where TDto : class, new()
        {
            if (tree is null)
                return query;
            var body = GetExpandExpression(tree.Root, parameter, tree, cache);

            var expr = ExpressionBuilder.BuildSelectExpression(query, parameter, body);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        public static IQueryable<TDto> ApplyFilter<TDto>(Tree tree, IQueryable<TDto> query, ParameterExpression parameter, Expression filterExpression) where TDto : class, new()
        {
            if (filterExpression is null) return query;

            var expr = ExpressionBuilder.BuildWhereExpression(query, parameter, filterExpression);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        public static IQueryable<TDto> ApplySelect<TDto>(Tree tree, IQueryable<TDto> query, ParameterExpression parameter, IClassCache cache) where TDto : class, new()
        {
            var selectExpression = GetSelectExpression(tree.Root, parameter, tree, cache) ?? throw new ArgumentNullException("Root filter node or resulting expression is null");

            if (selectExpression is null) return query;

            var expr = ExpressionBuilder.BuildSelectExpression(query, parameter, selectExpression);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        #endregion




        private static ICacheForever<int, List<MemberBinding>> _propertyBindingCache = new DictionaryCache<int, List<MemberBinding>>();
        private static IEnumerable<MemberBinding> BindingsForProperties(Vertex info, IEnumerable<PropertyInfo> properties, Expression dto)
        {
            // var hash = (info.GetHashCode() + properties.Sum(x => x.PropertyType.GetHashCode()) + dto.GetHashCode()) % int.MaxValue;
            // return _propertyBindingCache.GetOrAdd(info.Value.TypeId, () =>
            // {
            var result = new List<MemberBinding>();
            foreach (var prop in properties)
                result.Add(Expression.Bind(prop, Expression.PropertyOrField(dto, prop.Name)));
            return result;
            // });
        }

        #region Expressions for expansion

        public static Expression GetExpandExpression(Vertex info, Expression dto, Tree tree, IClassCache cache) => BindingExpression(info, dto, ExpansionMemberBindings(info, dto, tree, cache));

        private static IEnumerable<MemberBinding> ExpansionMemberBindings(Vertex info, Expression dto, Tree tree, IClassCache cache)
        {
            var classInfo = cache.Get(info.Value.TypeId);
            var outgoingEdges = tree.Children.Select(c => c.Item1);

            // Bind all the non-expandable types.
            foreach (var prop in BindingsForProperties(info, classInfo.NonExpandableProperties, dto))
                yield return prop;

            // navigation properties
            foreach (var prop in classInfo.NavigationProperties.Where(x => outgoingEdges.Any(c => c.Value.Name == x.Name)))
            {
                var navigationVertex = outgoingEdges.FirstOrDefault(x => x.Value.Name == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == navigationVertex).Item2;
                yield return (Expression.Bind(prop, GetExpandExpression(navigationVertex, Expression.PropertyOrField(dto, prop.Name), navigationTree, cache)));
            }

            // // collections
            foreach (var prop in classInfo.Collections.Where(x => outgoingEdges.Any(c => c.Value.Name == x.Name)))
            {
                if (!prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Length != 1)
                    continue;

                var collectionVertex = outgoingEdges.Single(x => x.Value.Name == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == collectionVertex).Item2;
                var collectionType = cache.GetTypeFromId(collectionVertex.Value.TypeId);

                var listType = typeof(List<>).MakeGenericType(collectionType);

                var childrenParameter = Expression.Parameter(collectionType, prop.Name);
                var childrenProperty = Expression.PropertyOrField(dto, prop.Name);
                var select = ExpressionBuilder.BuildSelectExpression(collectionType,
                    childrenProperty,
                    childrenParameter, GetExpandExpression(collectionVertex, childrenParameter, navigationTree, cache));
                var list = Expression.Condition(
                    Expression.Equal(childrenProperty, Expression.Constant(null)),
                    Expression.New(listType.GetConstructor(new Type[] { }), new Expression[] { }),
                    Expression.New(listType.GetConstructor(new Type[] { listType }), select)
                    );
                yield return (Expression.Bind(prop, list));
            }
        }

        #endregion

        #region Expressions for select

        public static Expression GetSelectExpression(Vertex info, Expression dto, Tree tree, IClassCache cache) => BindingExpression(info, dto, SelectionMemberBindings(info, dto, tree, cache));

        private static IEnumerable<MemberBinding> SelectionMemberBindings(Vertex info, Expression dto, Tree tree, IClassCache cache)
        {
            yield break;
            // var classInfo = cache.GetOrAdd(info.Value.TypeId);
            // var outgoingEdges = tree.Children.Select(c => c.Item1);

            // foreach (var prop in BindingsForProperties(info, classInfo.NonExpandableProperties.Where(p => tree.Root.Value.GetSelectProperties().Contains(p.Name)), dto))
            //     yield return prop;

            // // navigation properties
            // foreach (var prop in classInfo.NavigationProperties.Where(x => outgoingEdges.Any(c => c.Value.PropertyName == x.Name)))
            // {
            //     var navigationVertex = outgoingEdges.FirstOrDefault(x => x.Value.PropertyName == prop.Name).To;
            //     var navigationTree = tree.Children.First(c => c.Item1.To == navigationVertex).Item2;
            //     yield return (Expression.Bind(prop, GetSelectExpression(navigationVertex, Expression.PropertyOrField(dto, prop.Name), navigationTree, cache)));
            // }

            // // collections
            // foreach (var prop in classInfo.Collections.Where(x => outgoingEdges.Any(c => c.Value.PropertyName == x.Name)))
            // {
            //     if (!prop.PropertyType.IsGenericType && prop.PropertyType.GenericTypeArguments.Length != 1)
            //         continue;

            //     var collectionVertex = outgoingEdges.Single(x => x.Value.PropertyName == prop.Name).To;
            //     var navigationTree = tree.Children.First(c => c.Item1.To == collectionVertex).Item2;
            //     var listType = typeof(List<>).MakeGenericType(collectionVertex.Value.TypeId);

            //     var childrenParameter = Expression.Parameter(collectionVertex.Value.TypeId, prop.Name);
            //     var childrenProperty = Expression.PropertyOrField(dto, prop.Name);
            //     var select = ExpressionBuilder.BuildSelectExpression(collectionVertex.Value.TypeId,
            //         childrenProperty,
            //         childrenParameter, GetSelectExpression(collectionVertex, childrenParameter, navigationTree, cache));
            //     var list = Expression.Condition(
            //         Expression.Equal(childrenProperty, Expression.Constant(null)),
            //         Expression.New(listType.GetConstructor(new Type[] { }), new Expression[] { }),
            //         Expression.New(listType.GetConstructor(new Type[] { listType }), select)
            //         );
            //     yield return (Expression.Bind(prop, list));
            // }
        }

        #endregion

        public static Expression BindingExpression(Vertex info, Expression dto, IEnumerable<MemberBinding> bindings)
        {
            var newDto = Expression.New(dto.Type);
            var selectExpression = Expression.MemberInit(newDto, bindings);

            return Expression.Condition(Expression.Equal(dto, Expression.Constant(null)),
                Expression.Constant(null, selectExpression.Type), selectExpression);
        }
    }
}
