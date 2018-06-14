using System;
using BookingVCSkypeBot.Authentication.Models;

namespace BookingVCSkypeBot.Models
{
    [Serializable]
    public class MeetingModel
    {
        public AuthResult AuthResult { get; set; }
        public string LocationName { get; set; }
        public int PeopleCount { get; set; }
        public DateTime StartDateTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}