using BookingVCSkypeBot.Authentication.Models;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading.Tasks;

namespace BookingVCSkypeBot.Authentication
{
    public interface IAuthProvider
    {
        Task<string> GetAuthUrlAsync(AuthenticationOptions authOptions, string state);
        Task<AuthResult> GetTokenByAuthCodeAsync(AuthenticationOptions authOptions, string authorizationCode);
        Task<AuthResult> GetAccessToken(AuthenticationOptions authOptions, IDialogContext context);
        string Name
        {
            get;
        }
    }
}
