namespace BookingVCSkypeBot.Core.Entities
{
    public class VC : BaseEntity
    {
        public int LocationId { get; set; }
        public Location Location { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public int MaxPeople { get; set; }
        public int ComfortPeopleRangeStart { get; set; }
        public int ComfortPeopleRangeEnd { get; set; }
    }
}