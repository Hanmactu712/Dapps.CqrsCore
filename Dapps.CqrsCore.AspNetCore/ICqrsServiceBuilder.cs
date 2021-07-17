using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.AspNetCore
{
    /// <summary>
    /// Interface for CQRS Service
    /// </summary>
    public interface ICqrsServiceBuilder
    {
        IServiceCollection Services { get; }
    }
}
