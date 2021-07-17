using System;
using System.Collections.Generic;
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
        IReadOnlyList<TEntity> ListAll();
        IReadOnlyList<TEntity> List(ISpecification<TEntity> spec);
        TEntity Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        int Count(ISpecification<TEntity> spec);
        TEntity First(ISpecification<TEntity> spec);
        TEntity FirstOrDefault(ISpecification<TEntity> spec);
    }
}
