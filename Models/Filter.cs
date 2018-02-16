using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;

namespace nodata.Models
{
    public class Filter<TDto>
    {
        public bool count { get; set; }
        public int top { get; set; }
        public int skip { get; set; }
        public string filter { get; set; }
        public string select { get; set; }


        private IQueryable<TDto> _ApplyTop(IQueryable<TDto> query)
            => query.Take(top);

        private IQueryable<TDto> _ApplySkip(IQueryable<TDto> query)
            => query.Skip(top);

        private List<string> GetMatchingProperties()
        {
            var selected = select.Split(",").ToList();

            var result = selected.Intersect(typeof(TDto).GetProperties().Select(x => x.Name)).ToList();
            foreach(var line in result)
                Console.WriteLine(line);
            return result.ToList();
        }


        public IQueryable<TDto> ApplyTo(IQueryable<TDto> query)
        {
            GetMatchingProperties();
            return _ApplyTop(query);
            /*
            var param = Expression.Parameter(typeof(TDto));
            var body = Expression.Equal(
                Expression.Property(param, "Name"),
                Expression.Constant("one")
            );
            var expr = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { typeof(TDto) },
                query.Expression,
                Expression.Lambda(body, param)
            );
            return query.Provider.CreateQuery<TDto>(expr);
             */
            
            //queryable = queryable.Provider.CreateQuery<TDto>(parameterExpression);
            //return queryable;
        }
    }
}