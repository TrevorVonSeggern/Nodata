using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using QuickCache;
using NoData.GraphImplementations.Queryable;
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
        public static IQueryable<TDto> ApplySelectExpand<TDto>(ParameterExpression dtoParameter, Expression expr, IQueryable<TDto> query) where TDto : class, new()
        {
            var selectExpr = ExpressionBuilder.BuildSelectExpression(query, dtoParameter, expr);

            return query.Provider.CreateQuery<TDto>(selectExpr);
        }

        public static IQueryable<TDto> ApplyFilter<TDto>(QueryTree tree, IQueryable<TDto> query, ParameterExpression parameter, Expression filterExpression) where TDto : class, new()
        {
            if (filterExpression is null) return query;

            var expr = ExpressionBuilder.BuildWhereExpression(query, parameter, filterExpression);

            return query.Provider.CreateQuery<TDto>(expr);
        }

        #endregion

        // private static ICacheForever<int, List<MemberBinding>> _propertyBindingCache = new DictionaryCache<int, List<MemberBinding>>();
        private static IEnumerable<MemberBinding> BindingsForProperties(QueryVertex info, IClassCache cache, Expression dto)
        {
            // var hash = (info.GetHashCode() + properties.Sum(x => x.PropertyType.GetHashCode()) + dto.GetHashCode()) % int.MaxValue;
            // return _propertyBindingCache.GetOrAdd(info.Value.TypeId, () =>
            // {
            var result = new List<MemberBinding>();
            var classInfo = cache.ClassFromTypeId(info.Value.TypeId);
            foreach (var prop in info.Value.Properties.Where(x => x.IsPrimitive))
                result.Add(Expression.Bind(classInfo.NonExpandableProperties.First(x => x.Name == prop.Name), Expression.PropertyOrField(dto, prop.Name)));
            return result;
            // });
        }

        #region Expressions for expansion

        public static Expression GetExpandExpression(Expression dto, QueryTree tree, IClassCache cache) => BindingExpression(dto, ExpansionMemberBindings(dto, tree, cache));

        private static IEnumerable<MemberBinding> ExpansionMemberBindings(Expression dto, QueryTree tree, IClassCache cache)
        {
            var vertex = tree.Root;
            var classInfo = cache.Get(vertex.Value.TypeId);
            var outgoingEdges = tree.Children.Select(c => c.Item1);

            // Bind all the non-expandable types.
            foreach (var prop in BindingsForProperties(vertex, cache, dto))
                yield return prop;

            // navigation properties
            foreach (var prop in vertex.Value.Properties.Where(x => x.IsNavigationProperty))
            {
                var propertyInfo = classInfo.NavigationProperties.First(x => x.Name == prop.Name);
                var navigationVertex = outgoingEdges.FirstOrDefault(x => x.Value.Name == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == navigationVertex).Item2;
                yield return (Expression.Bind(propertyInfo, GetExpandExpression(Expression.PropertyOrField(dto, prop.Name), navigationTree, cache)));
            }

            // // collections
            foreach (var prop in vertex.Value.Properties.Where(x => x.IsCollection))
            {
                var propertyInfo = classInfo.Collections.First(x => x.Name == prop.Name);

                if (!propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GenericTypeArguments.Length != 1)
                    continue;

                var collectionVertex = outgoingEdges.Single(x => x.Value.Name == prop.Name).To;
                var navigationTree = tree.Children.First(c => c.Item1.To == collectionVertex).Item2;
                var collectionType = cache.GetTypeFromId(collectionVertex.Value.TypeId);

                var listType = typeof(List<>).MakeGenericType(collectionType);

                var childrenParameter = Expression.Parameter(collectionType, prop.Name);
                var childrenProperty = Expression.PropertyOrField(dto, prop.Name);
                var select = ExpressionBuilder.BuildSelectExpression(collectionType,
                    childrenProperty,
                    childrenParameter, GetExpandExpression(childrenParameter, navigationTree, cache));
                var list = Expression.Condition(
                    Expression.Equal(childrenProperty, Expression.Constant(null)),
                    Expression.New(listType.GetConstructor(new Type[] { }), new Expression[] { }),
                    Expression.New(listType.GetConstructor(new Type[] { listType }), select)
                    );
                yield return (Expression.Bind(propertyInfo, list));
            }
        }

        #endregion

        public static Expression BindingExpression(Expression dto, IEnumerable<MemberBinding> bindings)
        {
            // hash and cache?

            var newDto = Expression.New(dto.Type);
            var selectExpression = Expression.MemberInit(newDto, bindings);

            return Expression.Condition(Expression.Equal(dto, Expression.Constant(null)),
                Expression.Constant(null, selectExpression.Type), selectExpression);
        }
    }
}
