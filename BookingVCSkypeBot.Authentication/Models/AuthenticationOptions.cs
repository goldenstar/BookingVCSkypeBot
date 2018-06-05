using System;

namespace BookingVCSkypeBot.Authentication.Models
{
    [Serializable]
    public class AuthenticationOptions
    {
        [Obsolete("UseMagicNumber is deprecated and is a significant security vulnerability to disable.", false)]
        public bool UseMagicNumber { get; set; } = true;
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scopes { get; set; }
        public string RedirectUrl { get; set; }
        public string MagicNumberView { get; set; }
    }
}