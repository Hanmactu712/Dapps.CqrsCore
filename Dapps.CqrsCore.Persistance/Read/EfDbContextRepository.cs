using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Read
{
    public class EfDbContextRepository<TEntity> : EfRepository<TEntity, DbContext> where TEntity : class
    {
        public EfDbContextRepository(DbContext context) : base(context)
        {
        }
    }
}