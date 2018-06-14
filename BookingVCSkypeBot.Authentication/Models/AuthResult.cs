using System;

namespace BookingVCSkypeBot.Authentication.Models
{
    [Serializable]
    public class AuthResult
    {
        public string AccessToken { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public long ExpiresOnUtcTicks { get; set; }
        public byte[] TokenCache { get; set; }
    }
}