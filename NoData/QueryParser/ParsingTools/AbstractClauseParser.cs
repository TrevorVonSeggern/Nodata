using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Graph;
using NoData.GraphImplementations;
using NoData.QueryParser.ParsingTools.Groupers;

using QueueItem = NoData.GraphImplementations.QueryParser.Tree;
using TInfo = NoData.GraphImplementations.QueryParser.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public abstract class AbstractClaseParser<TRootQueryType, TResult> : IQueryClauseParser<TResult>
    {
        protected string QueryString;
        private readonly IReadOnlyDictionary<Regex, Func<IList<QueueItem>, ITuple<QueueItem, int>>> GroupingTerms;

        public Func<string, IList<QueueItem>> TokenFunc { get; }
        public IList<QueueItem> GetTokens(string parmeter) => TokenFunc(parmeter);

        public AbstractClaseParser(Func<string, IList<QueueItem>> tokenFunc, string query, IReadOnlyDictionary<Regex, Func<IList<QueueItem>, ITuple<QueueItem, int>>> groupingTerms)
        {
            QueryString = query;
            TokenFunc = tokenFunc;
            IsFinished = false;
            GroupingTerms = groupingTerms;
        }

        public virtual bool IsFinished { get; protected set; }
        public virtual Type RootQueryType => typeof(TRootQueryType);
        public virtual TResult Result { get; protected set; }

        protected IEnumerable<QueueItem> Tokens { get; private set; }
        public IGrouper<QueueItem> Grouper { get; private set; }
        protected bool SetupParsing()
        {
            IsFinished = true;
            if (string.IsNullOrWhiteSpace(QueryString))
                return false;
            Tokens = GetTokens(QueryString);

            if (!Tokens.Any())
                return false;

            Grouper = new OrderdGrouper<QueueItem>(GroupingTerms);
            // foreach (var grouping in GroupingTerms)
            //     Grouper.AddGroupingTerms(grouping.Item1, grouping.Item2);
            return true;
        }
        public abstract void Parse();
    }
}
