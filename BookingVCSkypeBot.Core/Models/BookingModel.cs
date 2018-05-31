using System;

namespace BookingVCSkypeBot.Core.Models
{
    [Serializable]
    public class BookingModel
    {
        public int TownName { get; set; }
        public int PeopleCount { get; set; }
        public DateTime DateTime { get; set; }
    }
}