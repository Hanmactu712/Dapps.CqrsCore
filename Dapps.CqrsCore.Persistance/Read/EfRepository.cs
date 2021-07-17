using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Read
{
    /// <summary>
    /// Generic class for all repositories which using EntityFrameworkCore
    /// </summary>
    /// <typeparam name="TEntity">Data Entity Class</typeparam>
    /// <typeparam name="TDbContext">DbContext which contains the Data Entity</typeparam>
    public class EfRepository<TEntity, TDbContext> : IEfRepository<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext
    {
        private readonly TDbContext _context;

        public EfRepository(TDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get entity by unique id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TEntity GetById(Guid id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<TEntity> ListAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        /// <summary>
        /// Get all entities which match with the entity specification
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public IReadOnlyList<TEntity> List(ISpecification<TEntity> spec)
        {
            var specResult = ApplySpecification(spec);
            return specResult.ToList();
        }

        /// <summary>
        /// Add new entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
            return entity;
        }

        /// <summary>
        /// Update the existing entity
        /// </summary>
        /// <param name="entity"></param>
        public void Update(TEntity entity)
        {
            var updateEntity = _context.Entry(entity);
            updateEntity.State = EntityState.Modified;
            _context.SaveChanges();

        }

        /// <summary>
        /// Delete an entity
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
            _context.SaveChanges();
        }

        /// <summary>
        /// Count the number of entities which matches with entity specification
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public int Count(ISpecification<TEntity> spec)
        {
            var specResult = ApplySpecification(spec);
            return specResult.Count();
        }

        /// <summary>
        /// Get first entity which matches with entity specification
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public TEntity First(ISpecification<TEntity> spec)
        {
            var specResult = ApplySpecification(spec);
            return specResult.First();

        }

        /// <summary>
        /// Get first or default entity which matches with entity specification
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public TEntity FirstOrDefault(ISpecification<TEntity> spec)
        {
            var specResult = ApplySpecification(spec);
            return specResult.FirstOrDefault();
        }

        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
        {
            var evaluator = new SpecificationEvaluator();
            //var evaluator = new SpecificationEvaluator<TEntity>();
            return evaluator.GetQuery(_context.Set<TEntity>().AsQueryable(), spec);
        }
    }
}
