using System;
using System.Threading.Tasks;
using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Identity.Client;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace BookingVCSkypeBot.Authentication.AADv2
{
    [Serializable]
    public class AuthProvider : IAuthProvider
    {
        public async Task<AuthResult> GetTokenByContextAsync(AuthenticationOptions authOptions, IDialogContext context)
        {
            var isValidAuth = IsValidAuth(context, out var authResult);

            if (!isValidAuth)
            {
                return null;
            }

            try
            {
                var tokenCache = new AuthTokenCache(authResult.TokenCache).GetCacheInstance();
                var client = new ConfidentialClientApplication(authOptions.ClientId, authOptions.RedirectUrl, new ClientCredential(authOptions.ClientSecret), tokenCache, null);
                var result = await client.AcquireTokenSilentAsync(authOptions.Scopes, client.GetUser(authResult.UserId));
                authResult = result.ToAuthResult(tokenCache);
                context.StoreAuthResult(authResult, this);
            }
            catch (Exception)
            {
                await context.PostAsync("Your credentials expired and could not be renewed automatically!");
                return null;
            }

            return authResult;
        }

        public async Task<string> GetAuthUrlAsync(AuthenticationOptions authOptions, string state)
        {
            var redirectUri = new Uri(authOptions.RedirectUrl);
            var tokenCache = new AuthTokenCache().GetCacheInstance();
            var client = new ConfidentialClientApplication(authOptions.ClientId, redirectUri.ToString(), new ClientCredential(authOptions.ClientSecret), tokenCache, null);
            var uri = await client.GetAuthorizationRequestUrlAsync(authOptions.Scopes, null, $"state={state}");
            return uri.ToString();
        }

        public async Task<AuthResult> GetTokenByAuthCodeAsync(AuthenticationOptions authOptions, string authorizationCode)
        {
            var tokenCache = new AuthTokenCache().GetCacheInstance();
            var client = new ConfidentialClientApplication(authOptions.ClientId, authOptions.RedirectUrl, new ClientCredential(authOptions.ClientSecret), tokenCache, null);
            var result = await client.AcquireTokenByAuthorizationCodeAsync(authorizationCode, authOptions.Scopes);
            var authResult = result.ToAuthResult(tokenCache);
            return authResult;
        }

        private static bool IsValidAuth(IBotData context, out AuthResult authResult)
        {
            return IsAuthResult(context, out authResult) && IsValidMagicNumber(context);
        }

        private static bool IsAuthResult(IBotData context, out AuthResult authResult)
        {
            return context.UserData.TryGetValue($"{ContextKey.AuthResult}", out authResult);
        }

        private static bool IsValidMagicNumber(IBotData context)
        {
            return context.UserData.TryGetValue($"{ContextKey.MagicNumberValidated}", out bool validated) && validated;
        }
    }
}