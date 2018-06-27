using System;

namespace BookingVCSkypeBot.Authentication.Models
{
    [Serializable]
    public class AuthenticationOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scopes { get; set; }
        public string RedirectUrl { get; set; }
    }
}