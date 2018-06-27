using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.ConnectorEx;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace BookingVCSkypeBot.Authentication.Helpers
{
    public class LoginCard
    {
        private readonly IAuthProvider authProvider;
        private readonly AuthenticationOptions authOptions;

        public LoginCard(IAuthProvider authProvider, AuthenticationOptions authOptions)
        {
            this.authProvider = authProvider;
            this.authOptions = authOptions;
        }

        public async Task<SigninCard> GetSinginCard(IDialogContext context)
        {
            var authenticationUrl = await GetAuthenticationUrlAsync(context);

            return new SigninCard("Login", GetCardActions(authenticationUrl));
        }

        private static List<CardAction> GetCardActions(string authenticationUrl)
        {
            var cardButtons = new List<CardAction>();

            var plButton = new CardAction
            {
                Value = authenticationUrl,
                Title = AuthRes.AuthenticationRequired,
                Type = "signin"
            };

            cardButtons.Add(plButton);

            return cardButtons;
        }

        private async Task<string> GetAuthenticationUrlAsync(IBotContext context)
        {
            var conversationRef = context.Activity.ToConversationReference();
            var state = GetState(conversationRef);
            return await authProvider.GetAuthUrlAsync(authOptions, state);
        }

        private string GetState(ConversationReference conversationRef)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            queryString["conversationRef"] = UrlToken.Encode(conversationRef);
            queryString["providerassembly"] = authProvider.GetType().Assembly.FullName;
            queryString["providertype"] = authProvider.GetType().FullName;

            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(queryString.ToString()));
        }
    }
}