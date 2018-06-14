using System.Collections.Generic;
using BookingVCSkypeBot.Core.Entities;
using BookingVCSkypeBot.Core.Interfaces;

namespace BookingVCSkypeBot.Core.Services
{
    public class VCService : IVCService
    {
        public IEnumerable<VC> VCListByLocationId(int locationId)
        {
            return new List<VC>();
        }
    }
}