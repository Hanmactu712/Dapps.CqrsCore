using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.Persistence.Store
{
    public class BaseStore<TContext>
    {
        private readonly IServiceProvider _service;

        public BaseStore(IServiceProvider service)
        {
            _service = service;
        }

        protected virtual TContext DbContext
        {
            get
            {
                var context = _service.CreateScope().ServiceProvider.GetRequiredService<TContext>();
                if (context == null)
                    throw new ArgumentNullException(nameof(TContext));
                return context;
            }
        }
    }
}
