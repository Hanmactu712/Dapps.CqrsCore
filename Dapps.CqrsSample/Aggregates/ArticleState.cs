using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsSample.EventHandlers;

namespace Dapps.CqrsSample.Aggregates
{
    public class ArticleState : AggregateState
    {
        public string UserName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        public ArticleState()
        {
        }

        public void When(ArticleCreated message)
        {
            AssignStateValue(message);
        }
    }
}
