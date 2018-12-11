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
        protected string QueryString { get; set; }
        private readonly IReadOnlyDictionary<Regex, Func<IList<QueueItem>, ITuple<QueueItem, int>>> GroupingTerms;

        public Func<string, IList<QueueItem>> TokenFunc { get; }
        public IList<QueueItem> GetTokens(string parmeter) => TokenFunc(parmeter);

        protected AbstractClaseParser(Func<string, IList<QueueItem>> tokenFunc, string query, IReadOnlyDictionary<Regex, Func<IList<QueueItem>, ITuple<QueueItem, int>>> groupingTerms)
        {
            QueryString = query;
            TokenFunc = tokenFunc ?? throw new ArgumentNullException(nameof(tokenFunc));
            IsFinished = false;
            GroupingTerms = groupingTerms;
        }

        public bool IsFinished { get; protected set; }
        public Type RootQueryType => typeof(TRootQueryType);
        public virtual TResult Result { get; protected set; }

        private IEnumerable<QueueItem> _tokenList { get; set; }
        public IGrouper<QueueItem> Grouper { get; private set; }
        protected bool SetupParsing()
        {
            IsFinished = true;
            if (string.IsNullOrWhiteSpace(QueryString))
                return false;
            _tokenList = GetTokens(QueryString);

            if (!_tokenList.Any())
                return false;

            Grouper = new OrderdGrouper<QueueItem>(GroupingTerms);

            return true;
        }
        public abstract void Parse();
    }
}
