using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using BookingVCSkypeBot.Core.Entities;
using BookingVCSkypeBot.Core.Interfaces;

namespace BookingVCSkypeBot.Infrastructure.Data
{
    public class EfRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly BookingVCContext dbContext;

        public EfRepository(BookingVCContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public T GetById(int id)
        {
            return dbContext.Set<T>().Find(id);
        }

        public IEnumerable<T> ListAll()
        {
            return dbContext.Set<T>().AsEnumerable();
        }

        public T Add(T entity)
        {
            dbContext.Set<T>().Add(entity);
            dbContext.SaveChanges();

            return entity;
        }

        public void Update(T entity)
        {
            dbContext.Entry(entity).State = EntityState.Modified;
            dbContext.SaveChanges();
        }

        public void Delete(T entity)
        {
            dbContext.Set<T>().Remove(entity);
            dbContext.SaveChanges();
        }
    }
}