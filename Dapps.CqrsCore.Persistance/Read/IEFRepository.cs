using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Read
{
    /// <summary>
    /// Interface of generic repository for create/update/delete/listing entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public interface IEfRepository<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext
    {
        TEntity GetById(Guid id);
        Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellation = default);
        IReadOnlyList<TEntity> ListAll();
        Task<IReadOnlyList<TEntity>> ListAllAsync(CancellationToken cancellation = default);
        IReadOnlyList<TEntity> List(ISpecification<TEntity> spec);
        Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> spec, CancellationToken cancellation = default);
        TEntity Add(TEntity entity);
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellation = default);
        void Update(TEntity entity);
        Task UpdateAsync(TEntity entity, CancellationToken cancellation = default);
        void Delete(TEntity entity);
        Task DeleteAsync(TEntity entity, CancellationToken cancellation = default);
        int Count(ISpecification<TEntity> spec);
        Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken cancellation = default);
        TEntity First(ISpecification<TEntity> spec);
        Task<TEntity> FirstAsync(ISpecification<TEntity> spec, CancellationToken cancellation = default);
        TEntity FirstOrDefault(ISpecification<TEntity> spec);
        Task<TEntity> FirstOrDefaultAsync(ISpecification<TEntity> spec, CancellationToken cancellation = default);
    }
}
