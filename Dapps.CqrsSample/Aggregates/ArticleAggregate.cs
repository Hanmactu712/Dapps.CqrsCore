using System;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsSample.EventHandlers;

namespace Dapps.CqrsSample.Aggregates
{
    public class ArticleAggregate : AggregateRoot
    {
        public override AggregateState CreateState()
        {
            return new ArticleState();
        }

        public void CreateArticle(string title, string summary, string details)
        {
            var ev = new ArticleCreated(Id, title, summary, details);

            Apply(ev);
        }
    }
}
