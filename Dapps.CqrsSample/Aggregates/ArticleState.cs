﻿using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsSample.EventHandlers;

namespace Dapps.CqrsSample.Aggregates;

public class ArticleState : AggregateState
{
    public string Title { get; private set; }
    public string Summary { get; private set; }
    public string Details { get; private set; }

    public ArticleState() { }

    public ArticleState(string title, string summary, string details)
    {
        Title = title;
        Summary = summary;
        Details = details;
    }

    public void When(ArticleCreated message)
    {
        AssignStateValue(message);
    }
    public void When(ArticleUpdated message)
    {
        AssignStateValue(message);
    }
}
