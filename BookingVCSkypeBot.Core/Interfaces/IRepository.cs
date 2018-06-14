using System.Collections.Generic;
using BookingVCSkypeBot.Core.Entities;

namespace BookingVCSkypeBot.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        T GetById(int id);
        IEnumerable<T> ListAll();
        T Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}