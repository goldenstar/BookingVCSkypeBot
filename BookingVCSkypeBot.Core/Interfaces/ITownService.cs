using System.Collections.Generic;
using BookingVCSkypeBot.Core.Entities;

namespace BookingVCSkypeBot.Core.Interfaces
{
    public interface ITownService : IBaseService
    {
        IEnumerable<TownEntity> ListAll();
    }
}
