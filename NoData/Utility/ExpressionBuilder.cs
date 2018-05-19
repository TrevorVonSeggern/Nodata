using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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

        public static Expression BuildOrderByExpression<TDto>(IQueryable<TDto> query, ParameterExpression parameter, Expression body)
        {
            return Expression.Call(
                typeof(Queryable), "OrderBy", new[] { typeof(TDto), typeof(TDto) },
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
    }
}
