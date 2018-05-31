using System;

namespace BookingVCSkypeBot.Models
{
    [Serializable]
    public class MeetingModel
    {
        public string TownName { get; set; }
        public int PeopleCount { get; set; }
        public DateTime StartDateTime { get; set; }
        public TimeSpan Duration { get; set; }

        public DateTime EndDateTime { get; set; }
    }
}