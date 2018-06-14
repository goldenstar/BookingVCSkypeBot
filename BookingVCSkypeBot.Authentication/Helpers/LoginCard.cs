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
            var authenticationUrl = await GetAuthenticationUrl(context);

            return new SigninCard("", GetCardActions(authenticationUrl));
        }

        private static List<CardAction> GetCardActions(string authenticationUrl)
        {
            var cardButtons = new List<CardAction>();

            var plButton = new CardAction
            {
                Value = authenticationUrl,
                Title = AuthRes.AuthenticationRequired
            };

            cardButtons.Add(plButton);

            return cardButtons;
        }

        private async Task<string> GetAuthenticationUrl(IBotContext context)
        {
            var conversationRef = context.Activity.ToConversationReference();
            var state = GetState(conversationRef);
            var authenticationUrl = await authProvider.GetAuthUrlAsync(authOptions, state);
            return authenticationUrl;
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