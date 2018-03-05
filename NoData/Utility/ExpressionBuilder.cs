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
            //return BuildSelectExpression(typeof(TDto), query.Expression, parameter, body);
        }

        public static Expression BuildSelectExpression<TDto>(Expression enumerable, ParameterExpression parameter, Expression body)
        {
            return Expression.Call(
                typeof(Enumerable), "Select", new[] { typeof(TDto), typeof(TDto) },
                enumerable,
                Expression.Lambda<Func<TDto, TDto>>(body, parameter)
            );
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
