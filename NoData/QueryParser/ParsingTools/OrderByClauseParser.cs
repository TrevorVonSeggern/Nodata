using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Graph;
using NoData.GraphImplementations.QueryParser;
using NoData.GraphImplementations.Schema;
using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using TInfo = NoData.GraphImplementations.QueryParser.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    class OrderByClauseParser<TRootQueryType> : AbstractClaseParser<TRootQueryType, IEnumerable<ITuple<PathToProperty, SortDirection>>>, IAcceptAdditions
    {
        private readonly GraphSchema Graph;
        private List<ITuple<PathToProperty, SortDirection>> ResultList = new List<ITuple<PathToProperty, SortDirection>>();
        public override IEnumerable<ITuple<PathToProperty, SortDirection>> Result => ResultList;

        public OrderByClauseParser(Func<string, IList<QueueItem>> tokenFunc, string query, GraphSchema graph, IReadOnlyDictionary<Regex, Func<IList<QueueItem>, ITuple<QueueItem, int>>> groupingTerms) : base(tokenFunc, query, groupingTerms)
        {
            Graph = graph;
        }

        public void AddToClause(string clause)
        {
            IsFinished = false;
            if (string.IsNullOrWhiteSpace(QueryString))
                QueryString = clause;
            else if (string.IsNullOrWhiteSpace(clause))
                return;
            else
                QueryString += "," + clause;
        }

        public void AddToClause(QueueItem clause)
        {
            const string invalidQueryText = "invalid query";

            if (clause is null)
                throw new ArgumentException(invalidQueryText);
            else if (clause.Representation == TextRepresentation.ListOfExpands || clause.Representation == TextRepresentation.ListOfSortings)
            {
                foreach (var child in clause.Children.Select(x => x.Item2))
                    AddToClause(child);
            }
            else if (clause.Representation == TextRepresentation.SortProperty)
            {
                var direction = clause.Children.Last().Item2.Root.Value.Text == "asc" ? SortDirection.Ascending : SortDirection.Descending;
                var child = clause.Children.First().Item2;
                ResultList.Add(ITuple.Create(SelectClauseParser<TRootQueryType>.PathAndPropertyFromExpandItem(child, Graph, RootQueryType), direction));
            }
            else if (clause.Representation == TextRepresentation.ExpandProperty)
                ResultList.Add(ITuple.Create(SelectClauseParser<TRootQueryType>.PathAndPropertyFromExpandItem(clause, Graph, RootQueryType), SortDirection.Ascending));
            else
                throw new ArgumentException($"{invalidQueryText} - Unrecognized term: {clause.Root.Value.Text}");
        }

        public override void Parse()
        {
            if (SetupParsing())
                AddToClause(Grouper.ParseToSingle(TokenFunc(QueryString)));
        }

        public static IQueryable<TDto> OrderBy<TDto>(IQueryable<TDto> query, IEnumerable<ITuple<PathToProperty, SortDirection>> orderByDefinitions, ParameterExpression parameter)
        {
            if (!orderByDefinitions.Any())
                return query;

            bool first = true;
            foreach (var sort in orderByDefinitions)
            {
                if (first)
                    query = query.OrderBy(parameter, OrderByFluentExpressions.GetOrderByExpression(parameter, sort.Item1), sort.Item2);
                else
                    query = query.ThenOrderBy(parameter, OrderByFluentExpressions.GetOrderByExpression(parameter, sort.Item1), sort.Item2);
                first = false;
            }
            return query;
        }
    }

    internal static class OrderByFluentExpressions
    {
        public static Expression GetOrderByExpression(Expression dto, PathToProperty sortPath)
        {
            Expression memberExpression = dto;
            sortPath.Traverse(x => memberExpression = Expression.PropertyOrField(memberExpression, x.Value.Name));

            var property = memberExpression.Type.GetProperty(sortPath.Property.Name);
            var propertyAccess = Expression.MakeMemberAccess(memberExpression, property);

            return propertyAccess;
        }

        public static IQueryable<TDto> OrderBy<TDto>(this IQueryable<TDto> source, ParameterExpression parameter, Expression propertyAccess, SortDirection sortOrder)
        {
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var typeArguments = new Type[] { typeof(TDto), propertyAccess.Type };
            var methodName = sortOrder == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var resultExp = Expression.Call(typeof(Queryable), methodName, typeArguments, source.Expression, Expression.Quote(orderByExp));

            return source.Provider.CreateQuery<TDto>(resultExp);
        }

        public static IQueryable<TDto> ThenOrderBy<TDto>(this IQueryable<TDto> source, ParameterExpression parameter, Expression propertyAccess, SortDirection sortOrder)
        {
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var typeArguments = new Type[] { typeof(TDto), propertyAccess.Type };
            var methodName = sortOrder == SortDirection.Ascending ? "ThenBy" : "ThenByDescending";
            var resultExp = Expression.Call(typeof(Queryable), methodName, typeArguments, source.Expression, Expression.Quote(orderByExp));

            return source.Provider.CreateQuery<TDto>(resultExp);
        }
    }
}
