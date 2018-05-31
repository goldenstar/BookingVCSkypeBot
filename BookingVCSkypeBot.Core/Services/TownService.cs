using System.Collections.Generic;
using BookingVCSkypeBot.Core.Entities;
using BookingVCSkypeBot.Core.Interfaces;

namespace BookingVCSkypeBot.Core.Services
{
    public class TownService : ITownService
    {
        public IEnumerable<TownEntity> ListAll()
        {
            var towns = new List<TownEntity>
            {
                new TownEntity {Id = 1, Name = "Minsk"},
                new TownEntity {Id = 2, Name = "Brest"},
                new TownEntity {Id = 3, Name = "Grodno"},
                new TownEntity {Id = 3, Name = "Gomel"}
            };

            return towns;
        }
    }
}