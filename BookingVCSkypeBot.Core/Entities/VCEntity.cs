namespace BookingVCSkypeBot.Core.Entities
{
    public class VCEntity : BaseEntity
    {
        public string Name { get; set; }
        public int MaxPeople { get; set; }
        public int ComfortPeopleRangeStart { get; set; }
        public int ComfortPeopleRangeEnd { get; set; }
    }
}