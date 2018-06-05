using System;
using System.Threading.Tasks;
using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Identity.Client;
using System.Diagnostics;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace BookingVCSkypeBot.Authentication.AADv2
{
    [Serializable]
    public class MSALAuthProvider : IAuthProvider
    {
        public string Name => "MSALAuthProvider";

        public async Task<AuthResult> GetAccessToken(AuthenticationOptions authOptions, IDialogContext context)
        {
            var validUser = IsValidUser(authOptions, context, out var authResult);

            if (!validUser)
            {
                return null;
            }

            try
            {
                var tokenCache = new InMemoryTokenCacheMSAL(authResult.TokenCache).GetMsalCacheInstance();
                var client = new ConfidentialClientApplication(authOptions.ClientId, authOptions.RedirectUrl, new ClientCredential(authOptions.ClientSecret), tokenCache, null);
                var result = await client.AcquireTokenSilentAsync(authOptions.Scopes, client.GetUser(authResult.UserUniqueId));
                authResult = result.FromMSALAuthenticationResult(tokenCache);
                context.StoreAuthResult(authResult, this);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to renew token: " + ex.Message);
                await context.PostAsync("Your credentials expired and could not be renewed automatically!");
                await Logout(authOptions, context);
                return null;
            }
            return authResult;
        }

        public async Task<string> GetAuthUrlAsync(AuthenticationOptions authOptions, string state)
        {
            var redirectUri = new Uri(authOptions.RedirectUrl);
            var tokenCache = new InMemoryTokenCacheMSAL().GetMsalCacheInstance();
            var client = new ConfidentialClientApplication(authOptions.ClientId, redirectUri.ToString(), new ClientCredential(authOptions.ClientSecret), tokenCache, null);
            var uri = await client.GetAuthorizationRequestUrlAsync(authOptions.Scopes, null, $"state={state}");
            return uri.ToString();
        }

        public async Task<AuthResult> GetTokenByAuthCodeAsync(AuthenticationOptions authOptions, string authorizationCode)
        {
            var tokenCache = new InMemoryTokenCacheMSAL().GetMsalCacheInstance();
            var client = new ConfidentialClientApplication(authOptions.ClientId, authOptions.RedirectUrl, new ClientCredential(authOptions.ClientSecret), tokenCache, null);
            var result = await client.AcquireTokenByAuthorizationCodeAsync(authorizationCode, authOptions.Scopes);
            var authResult = result.FromMSALAuthenticationResult(tokenCache);
            return authResult;
        }

        public async Task Logout(AuthenticationOptions authOptions, IDialogContext context)
        {
            context.UserData.RemoveValue($"{Name}{ContextConstants.AuthResultKey}");
            context.UserData.RemoveValue($"{Name}{ContextConstants.MagicNumberKey}");
            context.UserData.RemoveValue($"{Name}{ContextConstants.MagicNumberValidated}");
            var signoutURl = "https://login.microsoftonline.com/common/oauth2/logout?post_logout_redirect_uri=" + System.Net.WebUtility.UrlEncode(authOptions.RedirectUrl);
            await context.PostAsync($"In order to finish the sign out, please click at this [link]({signoutURl}).");
        }

        private bool IsValidUser(AuthenticationOptions authOptions, IBotData context, out AuthResult authResult)
        {
            return IsAuthResult(context, out authResult) && IsValidMagicNumber(authOptions, context);
        }

        private bool IsAuthResult(IBotData context, out AuthResult authResult)
        {
            return context.UserData.TryGetValue($"{Name}{ContextConstants.AuthResultKey}", out authResult);
        }

        private bool IsValidMagicNumber(AuthenticationOptions authOptions, IBotData context)
        {
            return !authOptions.UseMagicNumber 
                   || context.UserData.TryGetValue($"{Name}{ContextConstants.MagicNumberValidated}", out string validated) && validated == "true";
        }
    }
}