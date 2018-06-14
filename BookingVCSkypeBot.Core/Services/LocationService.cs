using System.Collections.Generic;
using BookingVCSkypeBot.Core.Entities;
using BookingVCSkypeBot.Core.Interfaces;

namespace BookingVCSkypeBot.Core.Services
{
    public class LocationService : ILocationService
    {
        private readonly IRepository<Location> locationRepository;

        public LocationService(IRepository<Location> locationRepository)
        {
            this.locationRepository = locationRepository;
        }

        public IEnumerable<Location> ListAll()
        {
            return locationRepository.ListAll();
        }
    }
}