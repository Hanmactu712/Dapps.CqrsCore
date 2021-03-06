using System;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsSample.EventHandlers;

namespace Dapps.CqrsSample.Aggregates
{
    public class ArticleAggregate : AggregateRoot
    {
        public override AggregateState CreateState()
        {
            return new ArticleState();
        }

        public void CreateArticle(string title, string summary, string details, Guid userId, Guid commandId)
        {
            var ev = new ArticleCreated(Id, title, summary, details, userId, commandId);

            Apply(ev);
        }

        public void UpdateArticle(Guid id, string title, string summary, string details, Guid userId, Guid commandId)
        {
            var ev = new ArticleUpdated(Id, title, summary, details, userId, commandId);

            Apply(ev);
        }

        public override void ApplyUnBoxingEvent(IEvent ev)
        {
            Apply(ev);
        }
    }
}
