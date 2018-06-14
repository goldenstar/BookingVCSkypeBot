using System.Collections.Generic;
using BookingVCSkypeBot.Core.Entities;

namespace BookingVCSkypeBot.Core.Interfaces
{
    public interface IVCService : IBaseService
    {
        IEnumerable<VC> VCListByLocationId(int locationId);
    }
}