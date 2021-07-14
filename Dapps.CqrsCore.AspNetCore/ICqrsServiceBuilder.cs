using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.AspNetCore
{
    public interface ICqrsServiceBuilder
    {
        IServiceCollection Services { get; }
    }
}
