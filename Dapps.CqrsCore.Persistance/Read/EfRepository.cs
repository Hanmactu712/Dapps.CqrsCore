using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Read
{
    public class EFRepository<TEntity, TDbContext> : IEFRepository<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext
    {
        private readonly TDbContext _context;

        public EFRepository(TDbContext context)
        {
            _context = context;
        }

        public TEntity GetById(Guid id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public IReadOnlyList<TEntity> ListAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        public IReadOnlyList<TEntity> List(ISpecification<TEntity> spec)
        {
            var specResult = ApplySpecification(spec);
            return specResult.ToList();
        }

        public TEntity Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public void Update(TEntity entity)
        {
            var updateEntity = _context.Entry(entity);
            updateEntity.State = EntityState.Modified;
            _context.SaveChanges();

        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
            _context.SaveChanges();
        }

        public int Count(ISpecification<TEntity> spec)
        {
            var specResult = ApplySpecification(spec);
            return specResult.Count();
        }

        public TEntity First(ISpecification<TEntity> spec)
        {
            var specResult = ApplySpecification(spec);
            return specResult.First();

        }

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
