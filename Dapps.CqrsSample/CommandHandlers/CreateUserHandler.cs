using Dapps.CqrsCore.Command;
using Dapps.CqrsCore.Event;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsSample.CommandHandlers
{
    public class CreateArticle : Command
    {
        public readonly string Title;
        public readonly string Summary;
        public readonly string Details;

        public CreateArticle(string title, string summary, string details)
        {
            Title = title;
            Summary = summary;
            Details = details;
        }
    }

    public class CreateUserHandler : CommandHandler<CreateArticle>
    {
        public CreateUserHandler(ICommandQueue queue, IEventRepository eventRepository, IEventQueue eventQueue,
            ILogger logger) : base(queue, eventRepository, eventQueue, logger)
        {
        }

        public override void Handle(CreateArticle command)
        {
            
        }
    }
}
