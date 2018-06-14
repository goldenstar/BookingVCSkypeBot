using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Identity.Client;

namespace BookingVCSkypeBot.Authentication.AADv2
{
    public static class Extensions
    {
        public static AuthResult ToAuthResult(this AuthenticationResult authResult, TokenCache tokenCache)
        {
            return new AuthResult
            {
                AccessToken = authResult.AccessToken,
                UserName = authResult.User.Name,
                UserId = authResult.User.Identifier,
                ExpiresOnUtcTicks = authResult.ExpiresOn.UtcTicks,
                TokenCache = tokenCache.Serialize()
            };
        }
    }
}