using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Read
{
    public class EFDbContextRepository<TEntity> : EFRepository<TEntity, DbContext> where TEntity : class
    {
        public EFDbContextRepository(DbContext context) : base(context)
        {
        }
    }
}