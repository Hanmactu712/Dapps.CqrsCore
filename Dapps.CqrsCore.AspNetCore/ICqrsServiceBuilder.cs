using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dapps.CqrsCore.AspNetCore
{
    /// <summary>
    /// Interface for CQRS Service
    /// </summary>
    public interface ICqrsServiceBuilder
    {
        IServiceCollection Services { get; }
        ILogger Logger { get; }
    }
}
