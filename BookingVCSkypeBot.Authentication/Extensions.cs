using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookingVCSkypeBot.Authentication
{
    public static class Extensions
    {
        public static void StoreAuthResult(this IBotContext context, AuthResult authResult, IAuthProvider authProvider)
        {
            context.UserData.SetValue($"{authProvider.Name}{ContextConstants.AuthResultKey}", authResult);
        }

        public static async Task<JObject> GetWithAuthAsync(this HttpClient client, string accessToken, string endpoint)
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            using (var response = await client.GetAsync(endpoint))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JObject.Parse(json);
            }
        }
    }
}
