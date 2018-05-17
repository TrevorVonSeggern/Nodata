using System;
using System.Collections.Generic;
using System.Linq;
using NoData.Graph;
using NoData.Graph.Base;
using NoData.QueryParser.ParsingTools.Groupers;
using QueueItem = NoData.QueryParser.Graph.Tree;
using TInfo = NoData.QueryParser.Graph.TextInfo;

namespace NoData.QueryParser.ParsingTools
{
    public abstract class AbstractClaseParser<TRootQueryType, TResult> : IQueryClaseParser<TResult>
    {
        protected string QueryString;
        private IList<ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>>> GroupingTerms = new List<ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>>>();

        public Func<string, IList<QueueItem>> TokenFunc { get; }
        public IList<QueueItem> GetTokens(string parmeter) => TokenFunc(parmeter);

        public AbstractClaseParser(Func<string, IList<QueueItem>> tokenFunc, string query)
        {
            QueryString = query;
            TokenFunc = tokenFunc;
            IsFinished = false;
        }

        public virtual bool IsFinished { get; protected set; }
        public virtual Type RootQueryType => typeof(TRootQueryType);
        public virtual TResult Result { get; protected set; }

        public void AddGroupingTerms(ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>> handleGrouping) => GroupingTerms.Add(handleGrouping);

        public void AddGroupingTerms(IEnumerable<ITuple<string, Func<IList<QueueItem>, ITuple<QueueItem, int>>>> handleGrouping)
        {
            foreach (var group in handleGrouping)
                AddGroupingTerms(group);
        }

        protected IEnumerable<QueueItem> Tokens { get; private set; }
        public IGrouper<QueueItem> Grouper { get; private set; }
        protected bool SetupParsing()
        {
            IsFinished = true;
            if (string.IsNullOrWhiteSpace(QueryString))
                return false;
            Tokens = GetTokens(QueryString);

            if (Tokens.Count() == 0)
                return false;

            Grouper = new OrderdGrouper<QueueItem>(GroupingTerms.ToDictionary(x => x.Item1, x => x.Item2));
            foreach (var grouping in GroupingTerms)
                Grouper.AddGroupingTerms(grouping.Item1, grouping.Item2);
            return true;
        }
        public abstract void Parse();
    }
}
