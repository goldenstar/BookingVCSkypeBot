using System.Collections.Generic;
using BookingVCSkypeBot.Core.Entities;

namespace BookingVCSkypeBot.Core.Interfaces
{
    public interface ILocationService : IBaseService
    {
        IEnumerable<Location> ListAll();
    }
}
